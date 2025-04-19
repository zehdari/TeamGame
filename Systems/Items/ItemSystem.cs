using ECS.Components.Items;
using ECS.Components.Tags;
using ECS.Components.Collision;
using ECS.Events;
using ECS.Core.Utilities;
using ECS.Core;

namespace ECS.Systems.Items;

public class ItemSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<ActionEvent>(HandlePickupInput); // Listen for grab key press
    }

    // Called when an action input (like "grab") is triggered
    private void HandlePickupInput(IEvent evt)
    {
        var actionEvent = (ActionEvent)evt;

        // Only act on the start of the "grab" action
        if (!actionEvent.IsStarted || actionEvent.ActionName != MAGIC.ACTIONS.PICK_UP)
            return;

        var player = actionEvent.Entity;

        // Ignore if player has no inventory
        if (!IsPlayer(player))
            return;

        // Find and pick up the nearest item
        FindAndPickupItem(player);
    }

    // Find nearby items using ContactState and pick up the first valid one
    private void FindAndPickupItem(Entity player)
    {
        if (HasComponents<ContactState>(player))
        {
            ref var contactState = ref GetComponent<ContactState>(player);
            
            if (contactState.Contacts != null && contactState.Contacts.Count > 0)
            {
                // Look for items in contact with the player
                foreach (var kvp in contactState.Contacts)
                {
                    Entity otherEntity = kvp.Key;
                    
                    // Check if the entity is an item
                    if (IsItem(otherEntity))
                    {
                        // Found an item, handle pickup
                        HandlePickup(otherEntity, player);
                        
                        // Only pick up one item at a time
                        break;
                    }
                }
            }
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

        // Publish item pickup event with the item entity
        Publish(new ItemPickupEvent(playerEntity, item, itemEntity));

        // Destroy the item entity after pickup
        Publish<DespawnEvent>(new DespawnEvent
        {
            Entity = itemEntity,
        });

        Publish<SoundEvent>(new SoundEvent
        {
            SoundKey = MAGIC.SOUND.ITEM_PICK_UP,
        });

    }

    public override void Update(World world, GameTime gameTime) { }
}