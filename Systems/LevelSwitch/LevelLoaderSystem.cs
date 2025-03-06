using ECS.Components.Items;
using ECS.Components.Animation;
using ECS.Components.Tags;
using ECS.Components.State;
using ECS.Core.Utilities;
using System;

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

            if (HasComponents<ObjectTag>(entity) ||
                HasComponents<PlayerTag>(entity) ||
                HasComponents<AITag>(entity))
                
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
