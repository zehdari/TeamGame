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

            // Clamp small forces to prevent underflow
            force.Value = ClampSmallValues(force.Value);
            velocity.Value = ClampSmallValues(velocity.Value);

            if (grounded.Value)
            {
                // Ground friction - only affects horizontal movement
                Vector2 groundNormal = grounded.GroundNormal;
                Vector2 tangent = new Vector2(-groundNormal.Y, groundNormal.X);
                tangent = Vector2.Normalize(tangent);

                // Project velocity onto the tangent
                float tangentialSpeed = Vector2.Dot(velocity.Value, tangent);
                Vector2 tangentialVelocity = tangent * tangentialSpeed;

                if (tangentialSpeed != 0)
                {
                    Vector2 frictionForce = -Vector2.Normalize(tangentialVelocity) *
                                            MathF.Abs(tangentialSpeed) * friction.Value;
                    force.Value += frictionForce;
                }

            }
        }
    }

    private Vector2 ClampSmallValues(Vector2 vector, float threshold = 1e-10f)
    {
        return new Vector2(
            Math.Abs(vector.X) < threshold ? 0 : vector.X,
            Math.Abs(vector.Y) < threshold ? 0 : vector.Y
        );
    }
}