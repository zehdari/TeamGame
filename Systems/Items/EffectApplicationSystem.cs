using ECS.Events;
using ECS.Components.Items;
using ECS.Core.Utilities;

namespace ECS.Systems.Items;

public class EffectApplicationSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<ItemPickupEvent>(HandleItemPickup); // Listen for ItemPickupEvent
    }

    // Handles applying the effect when an item is picked up
    private void HandleItemPickup(IEvent evt)
    {
        var itemPickupEvent = (ItemPickupEvent)evt;

        var player = itemPickupEvent.Player;
        var item = itemPickupEvent.Item;

        // Apply the item's effect (to be handled by external effect system)
        ApplyEffect(player, item.Value);
    }

    // Placeholder method to apply effects (integrate with Effect System later)
    private void ApplyEffect(Entity player, string effectName)
    {
        // TODO: Integrate with the Effects System once it's ready
        Console.WriteLine($"Applying effect '{effectName}' to player.");
    }

    public override void Update(World world, GameTime gameTime)
    {
        // No per-frame logic needed for applying item effects
    }
}
