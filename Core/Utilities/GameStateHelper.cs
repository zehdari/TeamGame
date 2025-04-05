using ECS.Components.State;
using ECS.Components.Tags;
using ECS.Core;

namespace ECS.Core.Utilities;

public static class GameStateHelper
{
    private static Entity GetGameStateEntity(World world)
    {
        return world.GetEntities()
            .FirstOrDefault(e => world.GetPool<GameStateComponent>().Has(e) &&
                                 world.GetPool<SingletonTag>().Has(e));
    }
    
    public static bool IsPaused(World world)
    {
        return CheckState(world, GameState.Paused);
    }
    
    public static bool IsMenu(World world)
    {
        return CheckState(world, GameState.MainMenu);
    }

    public static bool IsRunning(World world)
    {
        return CheckState(world, GameState.Running);
    }

    public static bool IsLevelSelect(World world)
    {
        return CheckState(world, GameState.LevelSelect);
    }

    public static bool IsTerminal(World world)
    {
        return CheckState(world, GameState.Terminal);
    }

    private static bool CheckState(World world, GameState gameState)
    {
        var gameStateEntity = GetGameStateEntity(world);

        if (!world.GetPool<GameStateComponent>().Has(gameStateEntity))
            return false;

        var currentGameState = world.GetPool<GameStateComponent>().Get(gameStateEntity).CurrentState;

        return gameState == currentGameState;
    }
    
    public static GameState GetGameState(World world)
    {
        var gameStateEntity = GetGameStateEntity(world);

        if (!world.GetPool<GameStateComponent>().Has(gameStateEntity))
            return GameState.Running; // Default state

        return world.GetPool<GameStateComponent>().Get(gameStateEntity).CurrentState;
    }
    
    public static bool SetGameState(World world, GameState newState)
    {
        var gameStateEntity = GetGameStateEntity(world);
        
        if (!world.GetPool<GameStateComponent>().Has(gameStateEntity))
            return false; // Failed to set state
        
        ref var stateComponent = ref world.GetPool<GameStateComponent>().Get(gameStateEntity);
        stateComponent.CurrentState = newState;
        
        return true; // Successfully set state
    }
}