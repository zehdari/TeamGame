using ECS.Components.Items;
using ECS.Components.Tags;
using ECS.Components.Collision;
using ECS.Events;
using ECS.Core.Utilities;
using System.ComponentModel.DataAnnotations;
using ECS.Core;

namespace ECS.Systems.Items;

public class ItemSystem : SystemBase
{
    // Tracks nearby items for each player (updated on collision)
    private Dictionary<Entity, List<Entity>> nearbyItems = new();

    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<CollisionEvent>(TrackNearbyItems); // Track overlap
        Subscribe<ActionEvent>(HandlePickupInput);     // Listen for grab key press
    }

    // Called when two entities collide
    private void TrackNearbyItems(IEvent evt)
    {
        var collisionEvent = (CollisionEvent)evt;
        var a = collisionEvent.Contact.EntityA;
        var b = collisionEvent.Contact.EntityB;

        //nearbyItems = new();

        // Add item to player's nearby list
        if (IsItem(a) && IsPlayer(b))
        {
            AddNearbyItem(b, a);
        }
        else if (IsItem(b) && IsPlayer(a))
        {
            AddNearbyItem(a, b);
        }
    }

    // Add an item to the player's list of nearby items
    private void AddNearbyItem(Entity player, Entity item)
    {
        if (!nearbyItems.ContainsKey(player))
            nearbyItems[player] = new List<Entity>();

        if (!nearbyItems[player].Contains(item))
            nearbyItems[player].Add(item);
    }

    // Called when an action input (like "grab") is triggered
    private void HandlePickupInput(IEvent evt)
    {
        var actionEvent = (ActionEvent)evt;

        // Only act on the start of the "grab" action
        if (!actionEvent.IsStarted || actionEvent.ActionName != MAGIC.ACTIONS.PICK_UP)
            return;

        var player = actionEvent.Entity;

        // Ignore if player has no inventory or no nearby items
        if (!IsPlayer(player) || !nearbyItems.ContainsKey(player))
            return;

        // Grab the first nearby item (can be expanded later)
        var item = nearbyItems[player].FirstOrDefault();
        if (item.Id == -1) return;

        if (World.GetEntities().Contains(item) && nearbyItems[player].Contains(item))
        {
            HandlePickup(item, player);

            // Remove item from tracking
            nearbyItems[player].Remove(item);
        } else if (nearbyItems[player].Contains(item))
        {
            nearbyItems.Remove(item);
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
        // Note: The effect system will get the effect components before this despawn happens
        Publish<DespawnEvent>(new DespawnEvent
        {
            Entity = itemEntity,
        });
    }

    public override void Update(World world, GameTime gameTime)
    {
    }
}
