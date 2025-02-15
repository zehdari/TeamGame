using ECS.Components.Objects;
using ECS.Components.Animation;
using ECS.Components.Tags;
using ECS.Components.State;
using ECS.Core.Utilities;

namespace ECS.Systems.Objects;

public class ObjectSwitchSystem : SystemBase
{
    private readonly Dictionary<string, int> actionDirections = new()
    {
        ["switch_object_forward"] = +1,
        ["switch_object_backward"] = -1
    };

    public override void Initialize(World world)
    {
        base.Initialize(world);
        World.EventBus.Subscribe<ActionEvent>(HandleObjectSwitchAction);
    }

    private void HandleObjectSwitchAction(IEvent evt)
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

        // Find all object entities and update their animation
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<ObjectTag>(entity) ||
                !HasComponents<MapObject>(entity) ||
                !HasComponents<AnimationState>(entity) ||
                !HasComponents<AnimationConfig>(entity))
                continue;

            ref var mapObject = ref GetComponent<MapObject>(entity);
            ref var animState = ref GetComponent<AnimationState>(entity);
            ref var animConfig = ref GetComponent<AnimationConfig>(entity);

            var availableStates = animConfig.States.Keys.ToArray();

            // Find the next animation state (object)
            int currentIndex = Array.IndexOf(availableStates, mapObject.Value);
            int nextIndex = (currentIndex + direction + availableStates.Length) % availableStates.Length;
            string newItem = availableStates[nextIndex];

            // Update object and animation state
            mapObject.Value = newItem;
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

