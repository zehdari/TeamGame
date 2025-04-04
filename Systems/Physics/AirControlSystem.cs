using ECS.Components.Physics;
using ECS.Components.State;

namespace ECS.Systems.Physics;

public class AirControlSystem : SystemBase
{
    private Dictionary<Entity, bool> isWalkingLeft = new();
    private Dictionary<Entity, bool> isWalkingRight = new();

    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<ActionEvent>(HandleAirMoveAction);
    }

    private void HandleAirMoveAction(IEvent evt)
    {
        var walkEvent = (ActionEvent)evt;

        if (walkEvent.ActionName.Equals("walk_left"))
        {
            if (!isWalkingLeft.ContainsKey(walkEvent.Entity))
                isWalkingLeft[walkEvent.Entity] = false;

            isWalkingLeft[walkEvent.Entity] = walkEvent.IsHeld;
        }

        if (walkEvent.ActionName.Equals("walk_right"))
        {
            if (!isWalkingRight.ContainsKey(walkEvent.Entity))
                isWalkingRight[walkEvent.Entity] = false;

            isWalkingRight[walkEvent.Entity] = walkEvent.IsHeld;
        }
    }

    public override void Update(World world, GameTime gameTime)
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<Force>(entity) ||
                !HasComponents<IsGrounded>(entity) ||
                !HasComponents<Velocity>(entity) ||
                !HasComponents<AirControlForce>(entity))
                continue;

            ref var force = ref GetComponent<Force>(entity);
            ref var grounded = ref GetComponent<IsGrounded>(entity);
            ref var velocity = ref GetComponent<Velocity>(entity);
            ref var airControl = ref GetComponent<AirControlForce>(entity);
            ref var state = ref GetComponent<PlayerStateComponent>(entity);

            if (state.CurrentState == PlayerState.Stunned)
                return;

            // Only apply when NOT grounded
            if (grounded.Value)
                continue;

            // Determine walking direction
            float direction = 0f;
            if (isWalkingLeft.TryGetValue(entity, out bool walkingLeft) && walkingLeft)
                direction -= 1f;
            if (isWalkingRight.TryGetValue(entity, out bool walkingRight) && walkingRight)
                direction += 1f;

            // Apply force based on walking direction
            if (direction != 0)
            {
                Vector2 airForce = new Vector2(direction * airControl.Value, 0);
                force.Value += airForce;
            }
        }
    }
}