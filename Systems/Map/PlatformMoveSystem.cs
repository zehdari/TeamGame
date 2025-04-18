using ECS.Components.Physics;
using ECS.Components.Map;
using ECS.Components.Collision;
using System.Diagnostics;

namespace ECS.Systems.Map;

public class PlatformMoveSystem : SystemBase
{
    private const float PLATFORM_SPEED = 50f;
    private const float POINT_THRESHOLD = 5f;
    private const int DIRECTION_CHANGE_GRACE_PERIOD = 15; // Frames to force collisions after direction change

    public override void Update(World world, GameTime gameTime)
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<PlatformRoute>(entity) ||
                !HasComponents<Velocity>(entity) ||
                !HasComponents<Position>(entity) ||
                !HasComponents<Platform>(entity))
                continue;

            ref var velocity = ref GetComponent<Velocity>(entity);
            ref var route = ref GetComponent<PlatformRoute>(entity);
            ref var position = ref GetComponent<Position>(entity);

            // Initialize direction state component if it doesn't exist
            if (!HasComponents<PlatformDirectionState>(entity))
            {
                World.GetPool<PlatformDirectionState>().Set(entity, new PlatformDirectionState
                {
                    WasMovingUp = false,
                    IsMovingUp = false,
                    JustChangedDirection = false,
                    DirectionChangeFrames = 0,
                    LastVelocityY = 0f
                });
            }

            ref var directionState = ref GetComponent<PlatformDirectionState>(entity);

            // Store previous velocity state
            directionState.WasMovingUp = directionState.IsMovingUp;
            directionState.LastVelocityY = velocity.Value.Y;

            if (route.Points.Count == 0)
                continue;

            if (route.CurrentIndex >= route.Points.Count)
                route.CurrentIndex = 0;

            var target = route.Points[route.CurrentIndex];
            var direction = target - position.Value;
            var distance = direction.Length();

            if (distance < POINT_THRESHOLD)
            {
                // Reached the point, go to the next one
                route.CurrentIndex = (route.CurrentIndex + 1) % route.Points.Count;
                target = route.Points[route.CurrentIndex];
                direction = target - position.Value;
                distance = direction.Length();
            }
            
            // Calculate and set velocity
            if (distance > 0f)
            {
                direction = Vector2.Normalize(direction);
                velocity.Value = direction * PLATFORM_SPEED;
            }
            else
            {
                velocity.Value = Vector2.Zero;
            }

            // Update direction state
            bool isMovingUp = velocity.Value.Y < 0f;
            directionState.IsMovingUp = isMovingUp;
            
            // Check for direction change
            bool directionChanged = (directionState.WasMovingUp && !isMovingUp) &&
                                    Math.Abs(directionState.LastVelocityY) > 0.1f;
                                    
            // Right now it's mostly just checking to see if its no longer up
            // As that was the problem child
            if (directionChanged)
            {
                directionState.JustChangedDirection = true;
                directionState.DirectionChangeFrames = DIRECTION_CHANGE_GRACE_PERIOD;
            }
            else if (directionState.DirectionChangeFrames > 0)
            {
                directionState.DirectionChangeFrames--;
                if (directionState.DirectionChangeFrames == 0)
                {
                    directionState.JustChangedDirection = false;
                }
            }
        }
    }
}
