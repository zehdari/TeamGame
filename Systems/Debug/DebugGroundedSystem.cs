using ECS.Components.Collision;
using ECS.Components.Physics;
using ECS.Components.Tags;

namespace ECS.Systems.Debug;

public class DebugGroundedSystem : SystemBase
{
    public override void Update(World world, GameTime gameTime)
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<IsGrounded>(entity))
                continue;

            ref var grounded = ref GetComponent<IsGrounded>(entity);
            ref var vel = ref GetComponent<Velocity>(entity);

            // Just output grounded state
            Console.WriteLine($"Entity {entity.Id} Debug:");
            Console.WriteLine($"  Grounded: {grounded.Value}");
            Console.WriteLine($"  WasGrounded: {grounded.WasGrounded}");
            Console.WriteLine($"  Velocity Y: {vel.Value.Y}");
        }
    }
}