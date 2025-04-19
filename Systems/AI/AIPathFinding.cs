using ECS.Components.Collision;
using ECS.Components.Physics;
using ECS.Components.Tags;
using ECS.Core.Debug;
using System.Collections.Generic;

namespace ECS.Systems.AI;

// Pathfinding that accounts for physics based movement and trajectories
public class PhysicsAwarePathfinding
{
    private readonly World world;
    
    // Pathfinding parameters
    private const int GRID_CELL_SIZE = 32;
    private const int MAX_PATH_LENGTH = 60;
    private const float PLATFORM_CLEARANCE = 50f;
    
    // Physics simulation constants
    private const float GRAVITY_ACCELERATION = 1000f; // Should match the GravitySystem
    private const float SIMULATION_STEP = 0.05f;      // Smaller steps for more accurate simulation
    private const float MAX_SIMULATION_TIME = 3.0f;   // Maximum time to simulate
    private const float JUMP_FORCE = 700f;            // Approx jump force
    private const float MOVE_FORCE = 300f;            // Approx horizontal move force
    
    // Cached grid representation of the level
    private bool[,] collisionGrid;
    private int gridWidth;
    private int gridHeight;
    private List<(Entity entity, Rectangle bounds)> platforms = new List<(Entity, Rectangle)>();
    private Rectangle stageBounds;
    
    public PhysicsAwarePathfinding(World world)
    {
        this.world = world;
        GenerateLevelRepresentation();
    }
    
    /// <summary>
    /// Generate a representation of the level for pathfinding
    /// </summary>
    public void GenerateLevelRepresentation()
    {
        // Scan for level boundaries and platforms
        Vector2 minBounds = new Vector2(float.MaxValue);
        Vector2 maxBounds = new Vector2(float.MinValue);
        platforms.Clear();
        
        foreach (Entity entity in world.GetEntities())
        {
            if (!world.GetPool<Position>().Has(entity))
                continue;
                
            ref var position = ref world.GetPool<Position>().Get(entity);
            
            bool isPlatform = world.GetPool<Platform>().Has(entity);
            bool isObject = world.GetPool<ObjectTag>().Has(entity);
            
            if (isPlatform || isObject)
            {
                // Get collision bounds
                Rectangle bounds = GetEntityBounds(entity, position);
                
                if (isPlatform)
                {
                    platforms.Add((entity, bounds));
                }
                
                // Update level bounds
                minBounds.X = Math.Min(minBounds.X, bounds.X);
                minBounds.Y = Math.Min(minBounds.Y, bounds.Y);
                maxBounds.X = Math.Max(maxBounds.X, bounds.X + bounds.Width);
                maxBounds.Y = Math.Max(maxBounds.Y, bounds.Y + bounds.Height);
            }
        }
        
        // Use default bounds if no objects found
        if (minBounds.X == float.MaxValue)
        {
            minBounds = new Vector2(0, 0);
            maxBounds = new Vector2(800, 600);
        }
        
        // Add some padding to stage bounds
        stageBounds = new Rectangle(
            (int)minBounds.X - 100, 
            (int)minBounds.Y - 100,
            (int)(maxBounds.X - minBounds.X) + 200,
            (int)(maxBounds.Y - minBounds.Y) + 200
        );
        
        // Calculate grid dimensions
        gridWidth = stageBounds.Width / GRID_CELL_SIZE + 1;
        gridHeight = stageBounds.Height / GRID_CELL_SIZE + 1;
        
        // Initialize grid
        collisionGrid = new bool[gridWidth, gridHeight];
        
        // Mark all edge cells
        for (int x = 0; x < gridWidth; x++)
        {
            collisionGrid[x, 0] = true;
            collisionGrid[x, gridHeight - 1] = true;
        }
        
        for (int y = 0; y < gridHeight; y++)
        {
            collisionGrid[0, y] = true;
            collisionGrid[gridWidth - 1, y] = true;
        }
        
        //Logger.Log($"Generated physics-aware pathfinding grid: {gridWidth}x{gridHeight}");
    }
    
    /// <summary>
    /// Get the approximate bounds of an entity
    /// </summary>
    private Rectangle GetEntityBounds(Entity entity, Position position)
    {
        if (world.GetPool<CollisionBody>().Has(entity))
        {
            ref var body = ref world.GetPool<CollisionBody>().Get(entity);
            return CalculateCollisionBounds(entity, body, position);
        }
        
        // Default size if no collision body
        return new Rectangle(
            (int)position.Value.X - 25,
            (int)position.Value.Y - 25,
            50, 50
        );
    }
    
    /// <summary>
    /// Calculate bounds from collision body
    /// </summary>
    private Rectangle CalculateCollisionBounds(Entity entity, CollisionBody body, Position position)
    {
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;
        
        foreach (var polygon in body.Polygons)
        {
            foreach (var vertex in polygon.Vertices)
            {
                // Apply entity position
                Vector2 worldVertex = vertex + position.Value;
                
                minX = Math.Min(minX, worldVertex.X);
                minY = Math.Min(minY, worldVertex.Y);
                maxX = Math.Max(maxX, worldVertex.X);
                maxY = Math.Max(maxY, worldVertex.Y);
            }
        }
        
        return new Rectangle(
            (int)minX,
            (int)minY,
            (int)(maxX - minX),
            (int)(maxY - minY)
        );
    }
    
    /// <summary>
    /// Convert world position to grid cell
    /// </summary>
    private (int, int) WorldToGrid(Vector2 position)
    {
        int gridX = (int)((position.X - stageBounds.X) / GRID_CELL_SIZE);
        int gridY = (int)((position.Y - stageBounds.Y) / GRID_CELL_SIZE);
        
        return (
            Math.Clamp(gridX, 0, gridWidth - 1),
            Math.Clamp(gridY, 0, gridHeight - 1)
        );
    }
    
    /// <summary>
    /// Convert grid cell to world position
    /// </summary>
    private Vector2 GridToWorld(int x, int y)
    {
        return new Vector2(
            stageBounds.X + (x * GRID_CELL_SIZE) + (GRID_CELL_SIZE / 2),
            stageBounds.Y + (y * GRID_CELL_SIZE) + (GRID_CELL_SIZE / 2)
        );
    }
    
    /// <summary>
    /// Plan a recovery path accounting for physics
    /// </summary>
    public RecoveryPlan PlanRecovery(Entity entity, Vector2 position, Vector2 velocity)
    {
        // Find a safe location on stage
        Vector2 safePosition = FindSafePosition(position);
        
        // Check if we're already above the safe position
        bool aboveSafePosition = position.Y < safePosition.Y;
        
        // Create plan based on relative position
        RecoveryPlan plan = new RecoveryPlan();
        
        // If off the left of the stage, go right
        if (position.X < stageBounds.X)
        {
            plan.HorizontalDirection = 1; // Right
            plan.HorizontalUrgency = Math.Abs(position.X - stageBounds.X) / 100f;
        }
        // If off the right of the stage, go left
        else if (position.X > stageBounds.X + stageBounds.Width)
        {
            plan.HorizontalDirection = -1; // Left
            plan.HorizontalUrgency = Math.Abs(position.X - (stageBounds.X + stageBounds.Width)) / 100f;
        }
        // If on stage horizontally, move toward safe position
        else
        {
            plan.HorizontalDirection = position.X < safePosition.X ? 1 : -1;
            plan.HorizontalUrgency = Math.Min(1.0f, Math.Abs(position.X - safePosition.X) / 200f);
        }
        
        // Estimate if jumping is needed to reach safe position
        if (position.Y > safePosition.Y + 50) // If below safe pos + clearance
        {
            // Estimate if current vertical momentum is enough
            float estimatedMaxHeight = EstimateMaxHeight(position.Y, velocity.Y);
            
            if (estimatedMaxHeight > safePosition.Y)
            {
                // Current momentum might be enough - don't jump yet
                plan.ShouldJump = false;
                plan.JumpUrgency = 0;
            }
            else
            {
                // Need to jump to get higher
                plan.ShouldJump = true;
                plan.JumpUrgency = Math.Min(1.0f, (position.Y - safePosition.Y) / 200f);
            }
        }
        // If already above or at safe position height
        else
        {
            plan.ShouldJump = false;
            plan.JumpUrgency = 0;
        }
        
        // If we'll land on a platform, check if drop-through is needed
        if (position.Y < safePosition.Y && 
            Math.Abs(position.X - safePosition.X) < 50 &&
            IsLikelyToLandOnPlatform(position, velocity, out Entity platformEntity))
        {
            // Check if we should go through this platform
            if (safePosition.Y > position.Y + 100)
            {
                plan.ShouldDropThrough = true;
            }
        }
        
        // Simulate the path to validate
        var (success, simulatedPath) = SimulatePath(entity, position, velocity, plan);
        if (!success)
        {
            // If simulation fails, try a different approach (e.g., more jumping)
            plan.ShouldJump = true;
            plan.JumpUrgency = 1.0f;
            
            // Re-simulate for debugging
            (success, simulatedPath) = SimulatePath(entity, position, velocity, plan);
        }
        
        // Store the simulated path for debug/visualization
        plan.SimulatedPath = simulatedPath;
        
        return plan;
    }
    
    /// <summary>
    /// Find a safe position on stage for recovery
    /// </summary>
    private Vector2 FindSafePosition(Vector2 position)
    {
        // By default, aim for center stage
        Vector2 centerStage = new Vector2(
            stageBounds.X + stageBounds.Width / 2,
            stageBounds.Y + stageBounds.Height / 2
        );
        
        // Find the nearest platform
        float bestDistance = float.MaxValue;
        Vector2 bestPosition = centerStage;
        
        foreach (var (platformEntity, bounds) in platforms)
        {
            // Calculate platform center
            Vector2 platformCenter = new Vector2(
                bounds.X + bounds.Width / 2,
                bounds.Y - 20 // Slightly above the platform
            );
            
            // Calculate distance (weighted to prefer platforms at similar heights)
            float heightDifference = Math.Abs(position.Y - platformCenter.Y);
            float horizontalDistance = Math.Abs(position.X - platformCenter.X);
            float weightedDistance = horizontalDistance + heightDifference * 2;
            
            if (weightedDistance < bestDistance)
            {
                bestDistance = weightedDistance;
                bestPosition = platformCenter;
            }
        }
        
        // If we're close enough to a platform, use it; otherwise use center stage
        return bestDistance < stageBounds.Width / 2 ? bestPosition : centerStage;
    }
    
    /// <summary>
    /// Estimate maximum height a character can reach with current vertical velocity
    /// </summary>
    private float EstimateMaxHeight(float currentY, float currentVelY)
    {
        // If moving upward, estimate peak height
        if (currentVelY < 0)
        {
            // Basic physics: y = y0 + v0*t + 0.5*a*t^2
            // At peak, v = 0, so t = -v0/a
            // Substitute to get peak height
            float timeToApex = -currentVelY / GRAVITY_ACCELERATION;
            float peakHeight = currentY + currentVelY * timeToApex + 
                              0.5f * GRAVITY_ACCELERATION * timeToApex * timeToApex;
            return peakHeight;
        }
        
        // If moving downward or stationary, current position is the peak
        return currentY;
    }
    
    /// <summary>
    /// Check if character will likely land on a platform with current trajectory
    /// </summary>
    private bool IsLikelyToLandOnPlatform(Vector2 position, Vector2 velocity, out Entity platformEntity)
    {
        platformEntity = new Entity(0);
        
        if (velocity.Y <= 0) // If moving upward or stationary
            return false;
            
        foreach (var (entity, bounds) in platforms)
        {
            // Project trajectory and check if it intersects platform
            float timeToReachPlatformY = (bounds.Y - position.Y) / velocity.Y;
            
            if (timeToReachPlatformY > 0 && timeToReachPlatformY < 1.0f)
            {
                float landingX = position.X + velocity.X * timeToReachPlatformY;
                
                if (landingX >= bounds.X && landingX <= bounds.X + bounds.Width)
                {
                    platformEntity = entity;
                    return true;
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Simulate character movement and physics to validate a recovery plan
    /// </summary>
    private (bool success, List<Vector2> path) SimulatePath(
        Entity entity, Vector2 position, Vector2 velocity, RecoveryPlan plan)
    {
        // Get physics properties of entity
        float mass = 1.0f;
        float airControl = 1.0f;
        float maxVelocity = 1000.0f;
        
        if (world.GetPool<Mass>().Has(entity))
            mass = world.GetPool<Mass>().Get(entity).Value;
            
        if (world.GetPool<AirControlForce>().Has(entity))
            airControl = world.GetPool<AirControlForce>().Get(entity).Value;
            
        if (world.GetPool<MaxVelocity>().Has(entity))
            maxVelocity = world.GetPool<MaxVelocity>().Get(entity).Value;
            
        // Start simulation from current state
        Vector2 simPosition = position;
        Vector2 simVelocity = velocity;
        List<Vector2> path = new List<Vector2> { simPosition };
        
        // Track if simulation reaches a safe position
        bool reachedSafe = false;
        Vector2 safePosition = FindSafePosition(position);
        float simulationTime = 0;
        
        // Simulate physics
        while (simulationTime < MAX_SIMULATION_TIME && !reachedSafe)
        {
            // Apply input forces based on plan
            Vector2 force = Vector2.Zero;
            
            // Gravity
            force.Y += GRAVITY_ACCELERATION * mass;
            
            // Horizontal movement
            if (plan.HorizontalUrgency > 0)
            {
                force.X += plan.HorizontalDirection * MOVE_FORCE * airControl * plan.HorizontalUrgency;
            }
            
            // Jump (only apply once at the start of simulation if needed)
            if (plan.ShouldJump && simulationTime < SIMULATION_STEP)
            {
                force.Y += -JUMP_FORCE * plan.JumpUrgency;
            }
            
            // Update velocity (F = ma, so a = F/m)
            simVelocity += (force / mass) * SIMULATION_STEP;
            
            // Clamp to max velocity
            if (simVelocity.Length() > maxVelocity)
            {
                simVelocity = Vector2.Normalize(simVelocity) * maxVelocity;
            }
            
            // Update position
            simPosition += simVelocity * SIMULATION_STEP;
            
            // Add to path
            path.Add(simPosition);
            
            // Check if we've reached a safe position (on stage)
            if (IsPositionSafe(simPosition, safePosition))
            {
                reachedSafe = true;
            }
            
            // Continue simulation
            simulationTime += SIMULATION_STEP;
        }
        
        return (reachedSafe, path);
    }
    
    /// <summary>
    /// Check if a position is safe (on stage and near target position)
    /// </summary>
    private bool IsPositionSafe(Vector2 position, Vector2 safePosition)
    {
        // Check if position is within stage bounds
        if (position.X < stageBounds.X || position.X > stageBounds.X + stageBounds.Width ||
            position.Y < stageBounds.Y || position.Y > stageBounds.Y + stageBounds.Height)
        {
            return false;
        }
        
        // Check if we're close to the safe position
        return Vector2.Distance(position, safePosition) < 100;
    }
    
    /// <summary>
    /// Convert a recovery plan to a list of executable actions
    /// </summary>
    public List<string> GetActionsFromPlan(RecoveryPlan plan)
    {
        var actions = new List<string>();
        
        // Horizontal movement
        if (plan.HorizontalDirection > 0)
        {
            actions.Add(MAGIC.ACTIONS.WALKRIGHT);
        }
        else if (plan.HorizontalDirection < 0)
        {
            actions.Add(MAGIC.ACTIONS.WALKLEFT);
        }
        
        // Jump if needed
        if (plan.ShouldJump)
        {
            actions.Add(MAGIC.ACTIONS.JUMP);
        }
        
        // Drop through if needed
        if (plan.ShouldDropThrough)
        {
            actions.Add(MAGIC.ACTIONS.DROP_THROUGH);
        }
        
        return actions;
    }
}

/// <summary>
/// Represents a recovery plan with movement directions and priorities
/// </summary>
public class RecoveryPlan
{
    // Direction: -1 for left, 1 for right, 0 for none
    public int HorizontalDirection { get; set; } = 0;
    
    // Urgency factors (0-1)
    public float HorizontalUrgency { get; set; } = 0;
    public float JumpUrgency { get; set; } = 0;
    
    // Action flags
    public bool ShouldJump { get; set; } = false;
    public bool ShouldDropThrough { get; set; } = false;
    
    // Debug path from simulation
    public List<Vector2> SimulatedPath { get; set; } = new List<Vector2>();
}
