using ECS.Components.Tags;

namespace ECS.Systems.Debug;

public class ActionEventDebugSystem : SystemBase
{
    public override void Update(World world, GameTime gameTime)
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<PlayerTag>(entity)) // Only debug player entities
                continue;
            World.EventBus.Publish(new ActionEvent
            {
                 ActionName = "run_right",
                 Entity = entity,
                 IsStarted = true,
                 IsEnded = false,
                 IsHeld = true
            });


        }
    }
}