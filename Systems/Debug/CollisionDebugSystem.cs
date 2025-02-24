using ECS.Components.Collision;
using ECS.Components.Physics;
using ECS.Components.Tags;

namespace ECS.Systems.Debug;

public class CollisionDebugSystem : SystemBase
{
    private Dictionary<Entity, List<CollisionEvent>> activeCollisions = new();
    private int collisionCount = 0;
    private float timeSinceLastLog = 0;
    private const float LOG_INTERVAL = 1.0f; // Log every second

    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<CollisionEvent>(HandleCollision);
    }

    private void HandleCollision(IEvent evt)
    {
        var collisionEvent = (CollisionEvent)evt;
        LogCollision(collisionEvent);

        // Track collisions for both entities
        TrackCollision(collisionEvent.Contact.EntityA, collisionEvent);
        TrackCollision(collisionEvent.Contact.EntityB, collisionEvent);
    }

    private void TrackCollision(Entity entity, CollisionEvent collisionEvent)
    {
        if (!activeCollisions.ContainsKey(entity))
        {
            activeCollisions[entity] = new List<CollisionEvent>();
        }

        if (collisionEvent.EventType == CollisionEventType.Begin)
        {
            activeCollisions[entity].Add(collisionEvent);
            collisionCount++;
        }
        else if (collisionEvent.EventType == CollisionEventType.End)
        {
            activeCollisions[entity].RemoveAll(c => 
                (c.Contact.EntityA.Id == collisionEvent.Contact.EntityA.Id && 
                 c.Contact.EntityB.Id == collisionEvent.Contact.EntityB.Id) ||
                (c.Contact.EntityA.Id == collisionEvent.Contact.EntityB.Id && 
                 c.Contact.EntityB.Id == collisionEvent.Contact.EntityA.Id));
            collisionCount--;
        }
    }

    private void LogCollision(CollisionEvent collision)
    {
        var entityA = collision.Contact.EntityA;
        var entityB = collision.Contact.EntityB;

        // Get velocities if they exist
        Vector2 velA = HasComponents<Velocity>(entityA) ? GetComponent<Velocity>(entityA).Value : Vector2.Zero;
        Vector2 velB = HasComponents<Velocity>(entityB) ? GetComponent<Velocity>(entityB).Value : Vector2.Zero;

        // Check if either entity is a player (for focused debugging)
        bool involvedPlayer = HasComponents<PlayerTag>(entityA) || HasComponents<PlayerTag>(entityB);

        if (involvedPlayer || collision.EventType == CollisionEventType.Begin)
        {
            Console.WriteLine($"\nCollision {collision.EventType}:");
            Console.WriteLine($"Entity A (ID: {entityA.Id}) - Vel: ({velA.X:F2}, {velA.Y:F2})");
            Console.WriteLine($"Entity B (ID: {entityB.Id}) - Vel: ({velB.X:F2}, {velB.Y:F2})");
            Console.WriteLine($"Normal: ({collision.Contact.Normal.X:F2}, {collision.Contact.Normal.Y:F2})");
            Console.WriteLine($"Penetration: {collision.Contact.Penetration:F2}");
            Console.WriteLine($"Layers: {collision.Contact.LayerA} vs {collision.Contact.LayerB}");
        }
    }

    public override void Update(World world, GameTime gameTime)
    {
        timeSinceLastLog += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (timeSinceLastLog >= LOG_INTERVAL)
        {
            // Log periodic stats
            Console.WriteLine($"\nCollision Stats:");
            Console.WriteLine($"Active Collisions: {collisionCount}");

            // Log player-specific collision info
            foreach (var entity in World.GetEntities())
            {
                if (!HasComponents<PlayerTag>(entity)) continue;

                ref var velocity = ref GetComponent<Velocity>(entity);
                ref var position = ref GetComponent<Position>(entity);
                bool isGrounded = HasComponents<IsGrounded>(entity) && GetComponent<IsGrounded>(entity).Value;

                Console.WriteLine($"\nPlayer Entity {entity.Id}:");
                Console.WriteLine($"Position: ({position.Value.X:F2}, {position.Value.Y:F2})");
                Console.WriteLine($"Velocity: ({velocity.Value.X:F2}, {velocity.Value.Y:F2})");
                Console.WriteLine($"Is Grounded: {isGrounded}");

                if (activeCollisions.ContainsKey(entity))
                {
                    Console.WriteLine("Current Collisions:");
                    foreach (var collision in activeCollisions[entity])
                    {
                        Entity other = collision.Contact.EntityA.Equals(entity) ? 
                            collision.Contact.EntityB : collision.Contact.EntityA;
                        Console.WriteLine($"- With Entity {other.Id} ({collision.Contact.LayerA} vs {collision.Contact.LayerB})");
                    }
                }
            }

            timeSinceLastLog = 0;
        }
    }
}