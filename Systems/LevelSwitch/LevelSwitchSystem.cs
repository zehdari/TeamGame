using ECS.Components.Items;
using ECS.Components.Animation;
using ECS.Components.Tags;
using ECS.Components.State;
using ECS.Core.Utilities;

namespace ECS.Systems.Items;

public class LevelSwitchSystem : SystemBase

{
    private GameStateManager gameStateManager;
    private List<string> levelSwitchNames = new List<string>();
    private int index = 1;

    string currentLevel = "DayLevel";
    bool hasChanged = false;

    public LevelSwitchSystem(GameStateManager stateManager)
    {
        this.gameStateManager = stateManager;
    }
    private readonly Dictionary<string, int> levelDirections = new()
    {
        ["switch_level_forward"] = +1,
    };

    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<ActionEvent>(HandleLevelSwitchAction);
        FillLevelList();
    }

    private void HandleLevelSwitchAction(IEvent evt)
    {

        

        var actionEvent = (ActionEvent)evt;

       
                // Ignore item switching if the game is paused
                if (GameStateHelper.IsPaused(World))
                    return;

                // Check if this is a level switch action
                if (!actionEvent.IsStarted)
                    return;

                if (!levelDirections.TryGetValue(actionEvent.ActionName, out int direction))
                    return;

               

                index += direction;
                index %= levelSwitchNames.Count;

        System.Diagnostics.Debug.WriteLine(index);

        currentLevel = levelSwitchNames[index];
        hasChanged = true;

        
    }
    private void FillLevelList()
    {
        levelSwitchNames.Add("DayLevel");
        levelSwitchNames.Add("NightLevel");
    }
    public override void Update(World world, GameTime gameTime) {
        if (hasChanged)
        {
            System.Diagnostics.Debug.WriteLine("Level swithed");
            gameStateManager.Initialize(currentLevel);
            hasChanged = false;
        }
    }
}
