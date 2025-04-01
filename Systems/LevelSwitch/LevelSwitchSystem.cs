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
    private int index = 0;
    bool needsToChange = false;
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

        System.Diagnostics.Debug.WriteLine(actionEvent.IsStarted);
        System.Diagnostics.Debug.WriteLine(actionEvent.IsEnded);

        if (!actionEvent.IsEnded)
            return;

        // Ignore state switching if the game isn't running
        if (!GameStateHelper.IsRunning(World))
            return;

        if (!levelDirections.TryGetValue(actionEvent.ActionName, out int direction))
            return;

        // Get the next index, wrapping around if going out of bounds
        index += direction;
        index %= levelSwitchNames.Count;

        needsToChange = true;
    }

    private void FillLevelList()
    {
        levelSwitchNames.Add("DayLevel");
        levelSwitchNames.Add("NightLevel");
        levelSwitchNames.Add("TestLevel");
        levelSwitchNames.Add("Roof");
        levelSwitchNames.Add("DayLevelArena");
        levelSwitchNames.Add("NightLevelArena");
    }

    public override void Update(World world, GameTime gameTime) {
        if (needsToChange)
        {
            // Get level
            var levelString = levelSwitchNames[index];

            Publish<LevelSwitchEvent>(new LevelSwitchEvent
            {
                Level = levelString
            });

            needsToChange = false;
        }
    }
}
