using ECS.Components.Physics;

namespace ECS.Systems.Physics;

public class FrictionSystem : SystemBase
{
    public override void Update(World world, GameTime gameTime)
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<Velocity>(entity) ||
                !HasComponents<Friction>(entity) ||
                !HasComponents<Force>(entity) ||
                !HasComponents<IsGrounded>(entity))
                continue;

            ref var velocity = ref GetComponent<Velocity>(entity);
            ref var friction = ref GetComponent<Friction>(entity);
            ref var force = ref GetComponent<Force>(entity);
            ref var grounded = ref GetComponent<IsGrounded>(entity);

            if (velocity.Value == Vector2.Zero)
                continue;

            if (grounded.Value)
            {
                // Ground friction - only affects horizontal movement
                Vector2 horizontalVel = new Vector2(velocity.Value.X, 0);
                if (horizontalVel != Vector2.Zero)
                {
                    Vector2 frictionForce = -Vector2.Normalize(horizontalVel) *
                        horizontalVel.Length() * friction.Value;
                    force.Value += frictionForce;
                }
            }
        }
    }
}