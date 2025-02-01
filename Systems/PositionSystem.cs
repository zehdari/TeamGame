namespace ECS.Systems;

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

            // Update position based on velocity
            position.Value += velocity.Value * deltaTime;
        }
    }
}