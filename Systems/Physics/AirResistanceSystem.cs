using ECS.Components.Physics;

namespace ECS.Systems.Physics;

public class AirResistanceSystem : SystemBase
{
    public override void Update(World world, GameTime gameTime)
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<Velocity>(entity) ||
                !HasComponents<AirResistance>(entity) ||
                !HasComponents<Force>(entity))
                continue;

            ref var velocity = ref GetComponent<Velocity>(entity);
            ref var airResistance = ref GetComponent<AirResistance>(entity);
            ref var force = ref GetComponent<Force>(entity);

            if (velocity.Value == Vector2.Zero)
                continue;

            // Apply air resistance in all directions
            Vector2 airResistanceForce = -velocity.Value * airResistance.Value;
            force.Value += airResistanceForce;
        }
    }
}