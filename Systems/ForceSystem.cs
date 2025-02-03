namespace ECS.Systems;


// Just converts force to acceleration
public class ForceSystem : SystemBase
{
    public override void Update(World world, GameTime gameTime)
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<Force>(entity) || 
                !HasComponents<Mass>(entity) ||
                !HasComponents<Acceleration>(entity))
                continue;

            ref var force = ref GetComponent<Force>(entity);
            ref var mass = ref GetComponent<Mass>(entity);
            ref var acceleration = ref GetComponent<Acceleration>(entity);

            // F = ma, therefore a = F/m
            acceleration.Value = force.Value / mass.Value;
            force.Value = Vector2.Zero;
        }
    }
}