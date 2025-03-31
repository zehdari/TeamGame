using ECS.Components.Items;
using ECS.Components.Animation;
using ECS.Components.Tags;
using ECS.Components.State;
using ECS.Core.Utilities;
using ECS.Components.UI;

namespace ECS.Systems.Items;

public class LevelLoaderSystem : SystemBase
    
{
    private GameStateManager gameStateManager;
    private LevelLoader level;
    private bool changeLevel = false;
    public LevelLoaderSystem(GameStateManager stateManager,LevelLoader level)
    {
        this.gameStateManager = stateManager;
        this.level = level;
        this.changeLevel = level.shouldChangeLevel;
    }

    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<LevelSwitchEvent>(HandleLevelSwitch);
    }

    private void clearCurrentEntities()
    {
        var entitySet= World.GetEntities();
        var entityArray = entitySet.ToArray();

        for (int i = 0; i < entityArray.Length; i++) { 

            var entity = entityArray[i];
            if (HasComponents<SingletonTag>(entity))
                continue;

            if (HasComponents<UIPaused>(entity))
            {
                var paused = GetComponent<UIPaused>(entity);
                if (paused.Value == true)
                    continue;
            }
                
            World.DestroyEntity(entity);
        }
    }

    private void HandleLevelSwitch(IEvent evt)
    {
        var levelEvent = (LevelSwitchEvent)evt;

        clearCurrentEntities();
        level.MakeEntities(levelEvent.Level);
        
    }
    public override void Update(World world, GameTime gameTime)
    {
    }

        

}
