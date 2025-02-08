using ECS.Components.State;
using ECS.Components.Tags;
using ECS.Core;

namespace ECS.Core.Utilities;

public static class GameStateHelper
{
    public static bool IsPaused(World world)
    {
        var gameStateEntity = world.GetEntities()
            .FirstOrDefault(e => world.GetPool<GameStateComponent>().Has(e) &&
                                 world.GetPool<SingletonTag>().Has(e));

        if (!world.GetPool<GameStateComponent>().Has(gameStateEntity)) // This shouldn't happen 🤞
            return false;

        var gameState = world.GetPool<GameStateComponent>().Get(gameStateEntity).CurrentState;

        return gameState == GameState.Paused;
    }
}
