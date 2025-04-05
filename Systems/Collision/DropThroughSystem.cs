using ECS.Components.Collision;
using ECS.Components.Physics;
using ECS.Components.Animation;

namespace ECS.Systems.Physics;

public class DropThroughSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<ActionEvent>(HandleDropThroughAction);
    }

    private void HandleDropThroughAction(IEvent evt)
    {
        var actionEvent = (ActionEvent)evt;
        
        // Only proceed if this is a "drop_through" action
        if (!actionEvent.ActionName.Equals(MAGIC.ACTIONS.DROP_THROUGH))
            return;
            
        var entity = actionEvent.Entity;
        
        // Initialize the component if needed
        if (!HasComponents<PlatformTraversalState>(entity))
        {
            World.GetPool<PlatformTraversalState>().Set(entity, new PlatformTraversalState
            {
                LastYPosition = GetComponent<Position>(entity).Value.Y,
                WasGoingUp = false,
                JustPassedUp = false,
                IsRequestingDropThrough = false,
                PassedThrough = new HashSet<int>()
            });
        }
        
        ref var traversalState = ref GetComponent<PlatformTraversalState>(entity);
        
        // Update the traversal state based on input action state
        if (actionEvent.IsStarted || actionEvent.IsHeld)
        {
            // When the key is pressed or held, mark that we want to drop through
            traversalState.IsRequestingDropThrough = true;
        }
        else if (actionEvent.IsEnded)
        {
            // Only when the key is released, clear the flag
            traversalState.IsRequestingDropThrough = false;
        }
    }

    public override void Update(World world, GameTime gameTime) { }
}