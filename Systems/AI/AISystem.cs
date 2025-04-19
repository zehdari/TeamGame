using ECS.Components.AI;
using ECS.Components.Physics;
using ECS.Components.Random;
using ECS.Components.Tags;
using ECS.Components.Timer;
using ECS.Components.Collision;
using ECS.Core.Debug;

namespace ECS.Systems.AI;

// This is very smelly, but I'm still just trying to get it working for now.
public class AISystem : SystemBase
{
    private const float DEFAULT_AI_TIMER_DURATION = 0.35f; // Slightly slower timer for less frantic behavior
    private Dictionary<int, string> actions = new();
    
    // Physics-aware pathfinding system
    private PhysicsAwarePathfinding pathfinding;
    
    // Track AI state and recovery plans
    private Dictionary<Entity, AIState> aiStates = new();
    private Dictionary<Entity, RecoveryPlan> recoveryPlans = new();
    private Dictionary<Entity, List<string>> actionQueue = new();
    
    // Action cooldowns to prevent jumpy behavior
    private Dictionary<Entity, Dictionary<string, float>> actionCooldowns = new();
    private const float JUMP_COOLDOWN = 1.5f; // Seconds before another jump
    private const float ACTION_SWITCH_COOLDOWN = 0.5f; // Seconds before switching actions
    
    // Constants for detection
    private const float EDGE_DETECTION_DISTANCE = 150f;
    private const float LEDGE_RAYCAST_DISTANCE = 120f; // Distance to check for ground below
    private const float RECOVERY_THRESHOLD = 100f;
    
    // Directional awareness
    private Dictionary<Entity, Vector2> stageDirections = new();
    
    // Visualization (for debugging)
    private Dictionary<Entity, List<Vector2>> debugPaths = new();
    private bool debugEnabled = true;
    
    // Stage analysis
    private Rectangle stageBounds;
    private List<Platform> platforms = new();
    private float stageFloorY = 600f; // Default floor level
    
    private enum AIState
    {
        Normal,     // Regular gameplay
        Recovering, // Trying to get back to stage
        Defensive,  // Avoiding danger/edges
        Attacking   // Engaging or approaching another entity
    }

    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<TimerEvent>(HandleTimerUp);
        Subscribe<CollisionEvent>(HandleCollision);
        MappingSetter();
        
        // Initialize physics-aware pathfinding
        pathfinding = new PhysicsAwarePathfinding(world);
        
        // Analyze stage - find platforms and estimate stage dimensions
        AnalyzeStage();
        
        //Logger.Log("AISystem initialized with physics-aware pathfinding");
    }
    
    private void AnalyzeStage()
    {
        platforms.Clear();
        Vector2 minBounds = new Vector2(float.MaxValue);
        Vector2 maxBounds = new Vector2(float.MinValue);
        
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<Position>(entity))
                continue;
                
            ref var position = ref GetComponent<Position>(entity);
            
            bool isPlatform = HasComponents<Platform>(entity);
            bool isStaticObject = HasComponents<ObjectTag>(entity);
            
            if (isPlatform || isStaticObject)
            {
                Rectangle bounds = GetEntityBounds(entity);
                
                if (isPlatform && HasComponents<Platform>(entity))
                {
                    platforms.Add(GetComponent<Platform>(entity));
                    
                    // Update floor level (highest Y value found on platforms)
                    stageFloorY = Math.Max(stageFloorY, bounds.Y + bounds.Height);
                }
                
                // Update stage bounds
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
            (int)minBounds.X - 50, 
            (int)minBounds.Y - 50,
            (int)(maxBounds.X - minBounds.X) + 100,
            (int)(maxBounds.Y - minBounds.Y) + 100
        );
        
        //Logger.Log($"Stage analysis complete. Floor Y: {stageFloorY}, Bounds: {stageBounds}");
    }
    
    private Rectangle GetEntityBounds(Entity entity)
    {
        if (!HasComponents<Position>(entity))
            return Rectangle.Empty;
            
        ref var position = ref GetComponent<Position>(entity);
        
        if (HasComponents<CollisionBody>(entity))
        {
            ref var body = ref GetComponent<CollisionBody>(entity);
            return CalculateCollisionBounds(entity, body, position);
        }
        
        // Default size if no collision body
        return new Rectangle(
            (int)position.Value.X - 25,
            (int)position.Value.Y - 25,
            50, 50
        );
    }
    
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

    private void MappingSetter()
    {
        int i = 0;
        actions.Add(i++, MAGIC.ACTIONS.JUMP);
        actions.Add(i++, MAGIC.ACTIONS.WALKLEFT);
        actions.Add(i++, MAGIC.ACTIONS.WALKRIGHT);
        actions.Add(i++, MAGIC.ACTIONS.SHOOT);
        actions.Add(i++, MAGIC.ACTIONS.DROP_THROUGH);
    }

    private void HandleCollision(IEvent evt)
    {
        var collision = (CollisionEvent)evt;
        
        // Ignore collision end events
        if (collision.EventType == CollisionEventType.End)
            return;
            
        // Check if this is an AI entity
        var entityA = collision.Contact.EntityA;
        var entityB = collision.Contact.EntityB;
        
        Entity aiEntity = default;
        Entity otherEntity = default;
        
        if (HasComponents<AITag>(entityA))
        {
            aiEntity = entityA;
            otherEntity = entityB;
        }
        else if (HasComponents<AITag>(entityB))
        {
            aiEntity = entityB;
            otherEntity = entityA;
        }
        else 
        {
            return; // No AI involved in this collision
        }
        
        // If this is a ground collision, update AI state
        if (collision.Contact.LayerA == CollisionLayer.World || 
            collision.Contact.LayerB == CollisionLayer.World ||
            HasComponents<Platform>(otherEntity))
        {
            // If AI was in recovering state, switch back to normal
            if (aiStates.TryGetValue(aiEntity, out var state) && state == AIState.Recovering)
            {
                //Logger.Log($"AI Entity {aiEntity.Id} recovery complete - back on stage");
                aiStates[aiEntity] = AIState.Normal;
                
                // Clear recovery plan
                if (recoveryPlans.ContainsKey(aiEntity))
                {
                    recoveryPlans.Remove(aiEntity);
                }
                
                // Clear action queue
                if (actionQueue.ContainsKey(aiEntity))
                {
                    actionQueue.Remove(aiEntity);
                }
            }
        }
    }

    private void HandleTimerUp(IEvent evt)
    {
        var timerEvent = (TimerEvent)evt;
        // Only process events for the AITimer
        if (timerEvent.TimerType != TimerType.AITimer)
            return;

        // Ensure the entity has the necessary AI components
        if (!HasComponents<AITag>(timerEvent.Entity) ||
            !HasComponents<CurrentAction>(timerEvent.Entity) ||
            !HasComponents<RandomlyGeneratedInteger>(timerEvent.Entity))
            return;

        // Retrieve components
        ref var action = ref GetComponent<CurrentAction>(timerEvent.Entity);
        ref var randomInt = ref GetComponent<RandomlyGeneratedInteger>(timerEvent.Entity);

        // Check if we should switch actions based on cooldowns
        bool canSwitchAction = !IsActionOnCooldown(timerEvent.Entity, "action_switch");
        
        if (canSwitchAction)
        {
            // End the previous action (if any)
            StopCurrentAction(timerEvent.Entity, action.Value);
            
            // Determine the next action based on AI state and recovery plans
            string newAction = DetermineNextAction(timerEvent.Entity, randomInt.Value);
            
            // Update current action
            action.Value = newAction;
            
            // Start the new action
            StartNewAction(timerEvent.Entity, newAction);
            
            // Set the action switch cooldown
            SetActionCooldown(timerEvent.Entity, "action_switch", ACTION_SWITCH_COOLDOWN);
            
            // Set specific action cooldowns
            if (newAction == MAGIC.ACTIONS.JUMP)
            {
                SetActionCooldown(timerEvent.Entity, MAGIC.ACTIONS.JUMP, JUMP_COOLDOWN);
            }
        }
        
        // Refresh pathfinding grid periodically
        if (randomInt.Value % 20 == 0)
        {
            AnalyzeStage();
            pathfinding.GenerateLevelRepresentation();
        }
    }
    
    private bool IsActionOnCooldown(Entity entity, string actionName)
    {
        if (actionCooldowns.TryGetValue(entity, out var cooldowns) && 
            cooldowns.TryGetValue(actionName, out float cooldown))
        {
            return cooldown > 0;
        }
        return false;
    }
    
    private void SetActionCooldown(Entity entity, string actionName, float duration)
    {
        if (!actionCooldowns.ContainsKey(entity))
        {
            actionCooldowns[entity] = new Dictionary<string, float>();
        }
        
        actionCooldowns[entity][actionName] = duration;
    }

    private void StopCurrentAction(Entity entity, string currentAction)
    {
        // Only publish end event if there was a current action
        if (!string.IsNullOrEmpty(currentAction))
        {
            World.EventBus.Publish(new ActionEvent
            {
                ActionName = currentAction,
                Entity = entity,
                IsStarted = false,
                IsEnded = true,
                IsHeld = false,
            });
        }
    }

    private void StartNewAction(Entity entity, string newAction)
    {
        // Ensure action isn't null before starting it
        if (string.IsNullOrEmpty(newAction))
        {
            Logger.Log("AI tried to call an action but it was null");
            return;
        }

        // Publish an ActionEvent to start the new action
        Publish(new ActionEvent
        {
            ActionName = newAction,
            Entity = entity,
            IsStarted = true,
            IsEnded = false,
            IsHeld = true,
        });
    }

    private string DetermineNextAction(Entity entity, int randomValue)
    {
        // Only continue if we have the required components
        if (!HasComponents<Position>(entity) || 
            !HasComponents<Velocity>(entity) || 
            !HasComponents<IsGrounded>(entity))
        {
            return GetRandomAction(randomValue);
        }

        ref var position = ref GetComponent<Position>(entity);
        ref var velocity = ref GetComponent<Velocity>(entity);
        ref var isGrounded = ref GetComponent<IsGrounded>(entity);

        // Get or initialize AI state for this entity
        if (!aiStates.TryGetValue(entity, out var aiState))
        {
            aiState = AIState.Normal;
            aiStates[entity] = aiState;
        }

        // Improved edge and off-stage detection using raycasting
        bool offStage = IsOffStage(entity, position.Value);
        bool nearEdge = IsNearEdge(entity, position.Value, velocity.Value);
        bool fallingDangerously = velocity.Value.Y > 300 && !isGrounded.Value;
        
        // Update stage direction for directional awareness
        UpdateStageDirection(entity, position.Value);
        
        // Update AI state based on analysis
        if (offStage || fallingDangerously)
        {
            // Switch to recovery mode if not already
            if (aiState != AIState.Recovering)
            {
                //Logger.Log($"AI Entity {entity.Id} entering recovery state at position {position.Value}");
                aiState = AIState.Recovering;
                
                // Create a new recovery plan
                var recoveryPlan = pathfinding.PlanRecovery(entity, position.Value, velocity.Value);
                recoveryPlans[entity] = recoveryPlan;
                
                // Store the actions needed for recovery
                var actions = pathfinding.GetActionsFromPlan(recoveryPlan);
                actionQueue[entity] = actions;
                
                // Save path for visualization
                if (debugEnabled)
                {
                    debugPaths[entity] = recoveryPlan.SimulatedPath;
                }
                
                //Logger.Log($"Created physics-based recovery plan with {actions.Count} actions");
            }
        }
        else if (nearEdge)
        {
            // Switch to defensive near edges
            aiState = AIState.Defensive;
        }
        else 
        {
            // Normal gameplay in safe areas
            aiState = AIState.Normal;
        }
        
        // Update stored state
        aiStates[entity] = aiState;

        // Choose action based on AI state
        switch (aiState)
        {
            case AIState.Recovering:
                return ChooseRecoveryAction(entity, position.Value, velocity.Value);
            
            case AIState.Defensive:
                return ChooseDefensiveAction(entity, position.Value, velocity.Value, isGrounded.Value);
            
            case AIState.Normal:
            default:
                return ChooseNormalAction(entity, position.Value, velocity.Value, isGrounded.Value, randomValue);
        }
    }
    
    private void UpdateStageDirection(Entity entity, Vector2 position)
    {
        // Calculate which direction is toward the stage center
        Vector2 stageCenter = new Vector2(
            stageBounds.X + stageBounds.Width / 2,
            stageBounds.Y + stageBounds.Height / 2
        );
        
        // Direction vector pointing to stage center
        Vector2 towardStage = stageCenter - position;
        towardStage.Normalize();
        
        // Store this direction for recovery
        stageDirections[entity] = towardStage;
    }

    private string ChooseRecoveryAction(Entity entity, Vector2 position, Vector2 velocity)
    {
        // If we have a recovery plan with an action queue, use next action
        if (actionQueue.TryGetValue(entity, out var actions) && actions.Count > 0)
        {
            string nextAction = actions[0];
            
            // If the action is Jump and on cooldown, try alternative
            if (nextAction == MAGIC.ACTIONS.JUMP && IsActionOnCooldown(entity, MAGIC.ACTIONS.JUMP))
            {
                if (actions.Count > 1)
                {
                    // Move to next action in queue
                    nextAction = actions[1];
                    actions.RemoveAt(1);
                }
                else if (velocity.Y > 0)
                {
                    // If falling, try to move horizontally toward stage instead
                    if (stageDirections.TryGetValue(entity, out var direction))
                    {
                        nextAction = direction.X > 0 ? MAGIC.ACTIONS.WALKRIGHT : MAGIC.ACTIONS.WALKLEFT;
                    }
                }
            }
            
            actions.RemoveAt(0);
            return nextAction;
        }
        
        // If we have a recovery plan but no actions (or used them all), regenerate
        if (recoveryPlans.TryGetValue(entity, out var plan))
        {
            // Regenerate actions from current plan
            var newActions = pathfinding.GetActionsFromPlan(plan);
            if (newActions.Count > 0)
            {
                string nextAction = newActions[0];
                newActions.RemoveAt(0);
                actionQueue[entity] = newActions;
                return nextAction;
            }
        }
        
        // Emergency recovery - use directional awareness to get back to stage
        if (stageDirections.TryGetValue(entity, out var stageDir))
        {
            // Jump if below stage level and moving toward stage horizontally
            bool needsVerticalRecovery = position.Y > stageFloorY && velocity.Y >= 0;
            if (needsVerticalRecovery && !IsActionOnCooldown(entity, MAGIC.ACTIONS.JUMP))
            {
                return MAGIC.ACTIONS.JUMP;
            }
            
            // Otherwise move horizontally toward stage
            return stageDir.X > 0 ? MAGIC.ACTIONS.WALKRIGHT : MAGIC.ACTIONS.WALKLEFT;
        }
        
        // Fallback - move toward stage center
        if (position.X < stageBounds.X + stageBounds.Width / 2)
        {
            return MAGIC.ACTIONS.WALKRIGHT;
        }
        else
        {
            return MAGIC.ACTIONS.WALKLEFT;
        }
    }

    private string ChooseDefensiveAction(Entity entity, Vector2 position, Vector2 velocity, bool isGrounded)
    {
        // Move away from edges toward center using directional awareness
        if (stageDirections.TryGetValue(entity, out var stageDir))
        {
            float centerX = stageBounds.X + stageBounds.Width / 2;
            
            // If definitely too close to edge, move toward center
            if (position.X < stageBounds.X + EDGE_DETECTION_DISTANCE || 
                position.X > stageBounds.X + stageBounds.Width - EDGE_DETECTION_DISTANCE)
            {
                return position.X < centerX ? MAGIC.ACTIONS.WALKRIGHT : MAGIC.ACTIONS.WALKLEFT;
            }
            
            // If moving too fast toward edge, consider jumping to switch momentum
            if ((velocity.X < -200 && position.X < centerX) || 
                (velocity.X > 200 && position.X > centerX))
            {
                if (isGrounded && !IsActionOnCooldown(entity, MAGIC.ACTIONS.JUMP))
                {
                    return MAGIC.ACTIONS.JUMP;
                }
            }
        }
        
        // Default: move toward stage center
        float stage_center_x = stageBounds.X + stageBounds.Width / 2;
        if (position.X < stage_center_x)
        {
            return MAGIC.ACTIONS.WALKRIGHT;
        }
        else
        {
            return MAGIC.ACTIONS.WALKLEFT;
        }
    }

    private string ChooseNormalAction(Entity entity, Vector2 position, Vector2 velocity, bool isGrounded, int randomValue)
    {
        // In normal gameplay, mix strategic actions with randomness but avoid jumpiness
        
        // Initialize weighted actions
        Dictionary<string, float> actionWeights = new Dictionary<string, float>
        {
            { MAGIC.ACTIONS.WALKRIGHT, 0.3f },
            { MAGIC.ACTIONS.WALKLEFT, 0.3f },
            { MAGIC.ACTIONS.JUMP, 0.05f }, // Much lower jump probability
            { MAGIC.ACTIONS.SHOOT, 0.1f },
            { MAGIC.ACTIONS.DROP_THROUGH, 0.02f }
        };
        
        // Adjust weights based on context
        
        // Determine position relative to stage center
        float centerX = stageBounds.X + stageBounds.Width / 2;
        float distanceFromCenter = Math.Abs(position.X - centerX);
        
        // Bias toward center when far away
        if (distanceFromCenter > 100)
        {
            if (position.X < centerX)
            {
                actionWeights[MAGIC.ACTIONS.WALKRIGHT] += 0.2f;
                actionWeights[MAGIC.ACTIONS.WALKLEFT] -= 0.1f;
            }
            else
            {
                actionWeights[MAGIC.ACTIONS.WALKLEFT] += 0.2f;
                actionWeights[MAGIC.ACTIONS.WALKRIGHT] -= 0.1f;
            }
        }
        
        // Increase jump weight if on ground and hasn't jumped recently
        if (isGrounded && !IsActionOnCooldown(entity, MAGIC.ACTIONS.JUMP))
        {
            actionWeights[MAGIC.ACTIONS.JUMP] += 0.1f;
            
            // Sometimes jump to change direction if moving too fast
            if ((velocity.X < -200 || velocity.X > 200) && randomValue % 10 < 2)
            {
                actionWeights[MAGIC.ACTIONS.JUMP] += 0.1f;
            }
        }
        else
        {
            // If not grounded or jump is on cooldown, don't jump
            actionWeights[MAGIC.ACTIONS.JUMP] = 0;
        }
        
        // Select action based on weights
        string selectedAction = null;
        float totalWeight = 0;
        foreach (var weight in actionWeights.Values)
        {
            totalWeight += weight;
        }
        
        float randomValue01 = (randomValue % 100) / 100.0f;
        float cumulativeWeight = 0;
        
        foreach (var action in actionWeights.Keys)
        {
            cumulativeWeight += actionWeights[action] / totalWeight;
            if (randomValue01 <= cumulativeWeight)
            {
                selectedAction = action;
                break;
            }
        }
        
        // Fallback
        return selectedAction ?? MAGIC.ACTIONS.WALKRIGHT;
    }

    private string GetRandomAction(int randomValue)
    {
        // Get a truly random action using the randomValue
        int actionIndex = randomValue % actions.Count;
        if (actions.TryGetValue(actionIndex, out string randomAction))
            return randomAction;
            
        // Default to walk right if no action found
        return MAGIC.ACTIONS.WALKRIGHT;
    }

    private bool IsOffStage(Entity entity, Vector2 position)
    {
        // Check if position is off the stage bounds
        if (position.X < stageBounds.X || 
            position.X > stageBounds.X + stageBounds.Width || 
            position.Y > stageBounds.Y + stageBounds.Height)
        {
            return true;
        }
        
        // Additional check - if AI is below all platforms and falling, consider off-stage
        if (position.Y > stageFloorY + 50 && 
            HasComponents<Velocity>(entity) && 
            GetComponent<Velocity>(entity).Value.Y > 0)
        {
            return true;
        }
        
        return false;
    }

    private bool IsNearEdge(Entity entity, Vector2 position, Vector2 velocity)
    {
        // Check if position is near a stage edge horizontally
        bool nearHorizontalEdge = (position.X < stageBounds.X + EDGE_DETECTION_DISTANCE && position.X >= stageBounds.X) || 
                                 (position.X > stageBounds.X + stageBounds.Width - EDGE_DETECTION_DISTANCE && position.X <= stageBounds.X + stageBounds.Width);
        
        // If not near horizontal edge, check for ledges using raycasting
        if (!nearHorizontalEdge && HasComponents<IsGrounded>(entity) && GetComponent<IsGrounded>(entity).Value)
        {
            // Get movement direction
            int movementDir = 0;
            if (Math.Abs(velocity.X) > 50)
            {
                movementDir = Math.Sign(velocity.X);
            }
            
            // Only check in movement direction
            if (movementDir != 0)
            {
                // Cast ray downward from in front of the AI
                Vector2 rayStart = position + new Vector2(movementDir * 50, 10);
                Vector2 rayEnd = rayStart + new Vector2(0, LEDGE_RAYCAST_DISTANCE);
                
                // Check if any platform intersects with ray
                bool foundGround = false;
                foreach (var platform in platforms)
                {
                    // Check platforms... still need to implement
                }
                
                // If no ground found in movement direction, we're near a ledge
                if (!foundGround)
                {
                    return true;
                }
            }
        }
        
        return nearHorizontalEdge;
    }

    public override void Update(World world, GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        // Update action cooldowns
        foreach (var entity in actionCooldowns.Keys.ToList())
        {
            var cooldowns = actionCooldowns[entity];
            foreach (var action in cooldowns.Keys.ToList())
            {
                cooldowns[action] -= deltaTime;
                if (cooldowns[action] <= 0)
                {
                    cooldowns.Remove(action);
                }
            }
            
            // Clean up empty cooldown dictionaries
            if (cooldowns.Count == 0)
            {
                actionCooldowns.Remove(entity);
            }
        }
        
        // Ensure AI entities have necessary components
        foreach (Entity entity in world.GetEntities())
        {
            if (!HasComponents<AITag>(entity))
                continue;

            // Ensure the entity has a Timers component
            if (!HasComponents<Timers>(entity))
            {
                World.GetPool<Timers>().Set(entity, new Timers
                {
                    TimerMap = new Dictionary<TimerType, Timer>()
                });
            }

            // Get the Timers component
            ref var timers = ref GetComponent<Timers>(entity);

            // If the AITimer isn't already added, add it
            if (!timers.TimerMap.ContainsKey(TimerType.AITimer))
            {
                timers.TimerMap[TimerType.AITimer] = new Timer
                {
                    Duration = DEFAULT_AI_TIMER_DURATION,
                    Elapsed = 0f,
                    Type = TimerType.AITimer
                };
            }
            
            // Update recovery plans when needed
            if (aiStates.TryGetValue(entity, out var state) && 
                state == AIState.Recovering &&
                (!actionQueue.ContainsKey(entity) || actionQueue[entity].Count == 0) &&
                HasComponents<Position>(entity) &&
                HasComponents<Velocity>(entity))
            {
                ref var position = ref GetComponent<Position>(entity);
                ref var velocity = ref GetComponent<Velocity>(entity);
                
                // Check if we need a new plan (significant position/velocity change or no plan)
                bool needNewPlan = !recoveryPlans.ContainsKey(entity);
                
                if (needNewPlan)
                {
                    var recoveryPlan = pathfinding.PlanRecovery(entity, position.Value, velocity.Value);
                    recoveryPlans[entity] = recoveryPlan;
                    
                    var actions = pathfinding.GetActionsFromPlan(recoveryPlan);
                    actionQueue[entity] = actions;
                    
                    // Store path for visualization
                    if (debugEnabled && recoveryPlan.SimulatedPath.Count > 0)
                    {
                        debugPaths[entity] = recoveryPlan.SimulatedPath;
                    }
                }
            }
        }
    }
}
