using ECS.Components.Physics;
using ECS.Components.UI;

namespace ECS.Systems.UI;

public class DamageSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        World.EventBus.Subscribe<ActionEvent>(HandlePercentChange);
    }

    private void HandlePercentChange(IEvent evt)
    {
        var percentChangeEvent = (ActionEvent)evt;

        if (!percentChangeEvent.ActionName.Equals("damage"))
        {
            return;
        }

        if (!HasComponents<Percent>(percentChangeEvent.Entity) ||
            !HasComponents<UIConfig>(percentChangeEvent.Entity))
        {
            return;
        }
            

        ref var percent = ref GetComponent<Percent>(percentChangeEvent.Entity);
        ref var uiConfig = ref GetComponent<UIConfig>(percentChangeEvent.Entity);

        
       
        percent.Value += 10;
        uiConfig.Text = percent.Value.ToString();
        uiConfig.Text += "%";
        
    }

    public override void Update(World world, GameTime gameTime) { }
}