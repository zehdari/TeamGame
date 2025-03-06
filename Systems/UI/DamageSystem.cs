using ECS.Components.Physics;
using ECS.Components.UI;
using ECS.Core.Utilities;

namespace ECS.Systems.UI;

public class DamageSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<ActionEvent>(HandlePercentChange);
    }

    private void HandlePercentChange(IEvent evt)
    {
        var percentChangeEvent = (ActionEvent)evt;

        if (!percentChangeEvent.ActionName.Equals("damage"))
        {
            return;
        }

        var entity = percentChangeEvent.Entity;

        if (!HasComponents<Percent>(entity) ||
            !HasComponents<UIText>(entity))
        {
            return;
        }
            
        if(percentChangeEvent.IsStarted)
        {
            ref var percent = ref GetComponent<Percent>(entity);
            ref var uiConfig = ref GetComponent<UIText>(entity);

            percent.Value += .1f;
        }

    }

    public override void Update(World world, GameTime gameTime) { }
}