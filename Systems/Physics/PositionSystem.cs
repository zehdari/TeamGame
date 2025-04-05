using ECS.Components.Physics;

namespace ECS.Systems.Physics;

public class PositionSystem : SystemBase
{
    public override void Update(World world, GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<Position>(entity) ||
                !HasComponents<Velocity>(entity))
                continue;

            ref var position = ref GetComponent<Position>(entity);
            ref var velocity = ref GetComponent<Velocity>(entity);

            // Skip if velocity is NaN
            if (float.IsNaN(velocity.Value.X) || float.IsNaN(velocity.Value.Y))
            {
                // Fix the velocity to prevent further issues
                Logger.Log($"PositionSystem: {entity.Id} had a NaN velocity, resetting to zero velocity.");
                velocity.Value = Vector2.Zero;
                continue;
            }

            // Update position based on velocity
            position.Value += velocity.Value * deltaTime;
        }
    }
}