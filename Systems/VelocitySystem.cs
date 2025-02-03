namespace ECS.Systems;

public class VelocitySystem : SystemBase
{
    public override void Update(World world, GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<Velocity>(entity) || 
                !HasComponents<Acceleration>(entity) ||
                !HasComponents<MaxVelocity>(entity))
                continue;

            ref var velocity = ref GetComponent<Velocity>(entity);
            ref var acceleration = ref GetComponent<Acceleration>(entity);
            ref var maxVelocity = ref GetComponent<MaxVelocity>(entity);

            // Update velocity based on acceleration
            velocity.Value += acceleration.Value * deltaTime;

            // No NaN's allowed here
            if (float.IsNaN(velocity.Value.X)) velocity.Value.X = 0;
            if (float.IsNaN(velocity.Value.Y)) velocity.Value.Y = 0;
            
            // // Clamp velocity to max speed
            // if (velocity.Value.Length() > maxVelocity.Value)
            // {
            //     velocity.Value = Vector2.Normalize(velocity.Value) * maxVelocity.Value;
            // }
        }
    }
}