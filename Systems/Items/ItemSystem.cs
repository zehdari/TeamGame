using ECS.Components.Items;
using ECS.Components.Tags;
using ECS.Components.Collision;
using ECS.Events;
using ECS.Core.Utilities;

namespace ECS.Systems.Items;

public class ItemSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<CollisionEvent>(HandleItemPickup); // Listen for CollisionEvent
    }

    // Handles item pickup when a collision occurs
    private void HandleItemPickup(IEvent evt)
    {
        var collisionEvent = (CollisionEvent)evt;

        var entityA = collisionEvent.Contact.EntityA;
        var entityB = collisionEvent.Contact.EntityB;

        // Check if either entity is an item and the other is a player
        if (IsItem(entityA) && IsPlayer(entityB))
        {
            HandlePickup(entityA, entityB);
        }
        else if (IsItem(entityB) && IsPlayer(entityA))
        {
            HandlePickup(entityB, entityA);
        }
    }

    // Checks if the entity is an item
    private bool IsItem(Entity entity)
    {
        return HasComponents<Item>(entity) && HasComponents<ItemTag>(entity);
    }

    // Checks if the entity is a player (by checking for inventory or other player tag)
    private bool IsPlayer(Entity entity)
    {
        return HasComponents<Inventory>(entity);
    }

    // Handles item pickup and adds item to player inventory
    private void HandlePickup(Entity itemEntity, Entity playerEntity)
    {
        ref var item = ref GetComponent<Item>(itemEntity);
        ref var inventory = ref GetComponent<Inventory>(playerEntity);

        // Add item to inventory
        inventory.CollectedItems.Add(item);

        // Publish item pickup event
        Publish(new ItemPickupEvent(playerEntity, item));

        // Destroy the item entity after pickup
        World.DestroyEntity(itemEntity);
    }

    public override void Update(World world, GameTime gameTime)
    {
    }
}
