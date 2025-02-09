using ECS.Components.Items;
using ECS.Components.Animation;
using ECS.Components.Tags;
using ECS.Components.State;
using ECS.Core.Utilities;

namespace ECS.Systems.Items;

public class ItemSwitchSystem : SystemBase
{
    private readonly Dictionary<string, int> actionDirections = new()
    {
        ["switch_item_forward"] = +1,
        ["switch_item_backward"] = -1
    };

    public override void Initialize(World world)
    {
        base.Initialize(world);
        World.EventBus.Subscribe<ActionEvent>(HandleItemSwitchAction);
    }

    private void HandleItemSwitchAction(IEvent evt)
    {
        var actionEvent = (ActionEvent)evt;

        // Ignore item switching if the game is paused
        if (GameStateHelper.IsPaused(World))
            return;

        // Check if this is an item switch action
        if (!actionEvent.IsStarted)
            return;

        if (!actionDirections.TryGetValue(actionEvent.ActionName, out int direction))
            return;

        // Find all item entities and update their animation
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<ItemTag>(entity) ||
                !HasComponents<Item>(entity) ||
                !HasComponents<AnimationState>(entity) ||
                !HasComponents<AnimationConfig>(entity))
                continue;

            ref var item = ref GetComponent<Item>(entity);
            ref var animState = ref GetComponent<AnimationState>(entity);
            ref var animConfig = ref GetComponent<AnimationConfig>(entity);

            var availableStates = animConfig.States.Keys.ToArray();

            // Find the next animation state (item)
            int currentIndex = Array.IndexOf(availableStates, item.Value);
            int nextIndex = (currentIndex + direction + availableStates.Length) % availableStates.Length;
            string newItem = availableStates[nextIndex];

            // Update item and animation state
            item.Value = newItem;
            animState.CurrentState = newItem;

            // Publish an animation state change event
            World.EventBus.Publish(new AnimationStateEvent 
            {
                Entity = entity,
                NewState = newItem
            });

        }
    }
    public override void Update(World world, GameTime gameTime) { }
}