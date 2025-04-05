using ECS.Components.Items;

namespace ECS.Events;

public struct ItemPickupEvent : IEvent
{
    public Entity Player;
    public Item Item;

    public ItemPickupEvent(Entity player, Item item)
    {
        Player = player;
        Item = item;
    }
}
