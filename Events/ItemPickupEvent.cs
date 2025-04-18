using ECS.Core;
using ECS.Components.Items;
using ECS.Events;

namespace ECS.Events;

public struct ItemPickupEvent : IEvent
{
    public Entity Player;
    public Item Item;
    public Entity ItemEntity;

    public ItemPickupEvent(Entity player, Item item, Entity itemEntity)
    {
        Player = player;
        Item = item;
        ItemEntity = itemEntity;
    }
}