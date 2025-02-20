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
            if (!HasComponents<IsGrounded>(entity) || 
                !HasComponents<CollisionState>(entity) ||
                !HasComponents<PlayerTag>(entity))  // Only debug player entities
                continue;

            ref var grounded = ref GetComponent<IsGrounded>(entity);
            ref var state = ref GetComponent<CollisionState>(entity);
            ref var vel = ref GetComponent<Velocity>(entity);

            bool hasGroundContact = (state.Sides & CollisionFlags.Bottom) != 0;

            Console.WriteLine($"Entity {entity.Id} Debug:");
            Console.WriteLine($"  Collision Sides: {state.Sides}");
            Console.WriteLine($"  Has Ground Contact: {hasGroundContact}");
            Console.WriteLine($"  Grounded.Value: {grounded.Value}");
            Console.WriteLine($"  WasGrounded: {grounded.WasGrounded}");
            Console.WriteLine($"  Velocity Y: {vel.Value.Y}");
            Console.WriteLine($"  Colliding With {state.CollidingWith.Count} entities");
        }
    }
}