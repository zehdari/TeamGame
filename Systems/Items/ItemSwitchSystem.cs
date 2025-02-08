using ECS.Components.Items;
using ECS.Components.Animation;
using ECS.Components.Tags;

namespace ECS.Systems.Items;
public class ItemSwitchSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        World.EventBus.Subscribe<ActionEvent>(HandleItemSwitchAction);
    }

    private void HandleItemSwitchAction(IEvent evt)
    {
        var actionEvent = (ActionEvent)evt;

        // Check if this is an item switch action
        if (!actionEvent.ActionName.Equals("switch_item") || !actionEvent.IsStarted)
            return;

        // Find all item entities
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

            // Get all states from the animation configuration
            var availableStates = animConfig.States.Keys.ToArray();

            // Find the current index and move to the next item state
            int currentIndex = Array.IndexOf(availableStates, item.Value);
            int nextIndex = (currentIndex + 1) % availableStates.Length;
            string newItem = availableStates[nextIndex];

            // Update item and animation state
            item.Value = newItem;
            animState.CurrentState = newItem;
            animState.FrameIndex = 0;
            animState.TimeInFrame = 0;

            // Publish an animation state change event to notify AnimationSystem
            World.EventBus.Publish(new AnimationStateEvent 
            {
                Entity = entity,
                NewState = newItem
            });

        }
    }
    public override void Update(World world, GameTime gameTime) { }
}