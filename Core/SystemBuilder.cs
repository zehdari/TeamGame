using ECS.Systems.AI;
using ECS.Systems.Animation;
using ECS.Systems.Collision;
using ECS.Systems.Input;
using ECS.Systems.Physics;
using ECS.Systems.Projectile;
using ECS.Systems.State;
using ECS.Systems.Utilities;
using ECS.Systems.Items;
using ECS.Systems.Characters;
using ECS.Systems.Debug;
using ECS.Systems.UI;
using ECS.Systems.Objects;

namespace ECS.Core;

public static class SystemBuilder
{

    public static void BuildSystems(World world, GameStateManager gameStateManager, GameAssets assets, GraphicsManager graphicsManager)
    {
        AddInputSystems(world);
        AddPreUpdateSystems(world, gameStateManager, assets);
        AddUpdateSystems(world);
        AddPostUpdateSystems(world, assets);
        AddRenderSystems(world, assets, graphicsManager);
    }

    private static void AddInputSystems(World world)
    {
        // Input Phase - Handle raw input and generate events
        world.AddSystem(new RawInputSystem(), SystemExecutionPhase.Input, 1);
        world.AddSystem(new InputMappingSystem(), SystemExecutionPhase.Input, 2);
    }

    private static void AddPreUpdateSystems(World world, GameStateManager gameStateManager, GameAssets assets)
    {
        // PreUpdate Phase - Handle input events and generate forces
        world.AddSystem(new GameStateSystem(gameStateManager), SystemExecutionPhase.PreUpdate, 0);
        world.AddSystem(new RandomSystem(), SystemExecutionPhase.PreUpdate, 1);
        world.AddSystem(new TimerSystem(), SystemExecutionPhase.PreUpdate, 2);
        world.AddSystem(new AISystem(), SystemExecutionPhase.PreUpdate, 3);
        world.AddSystem(new ProjectileSystem(), SystemExecutionPhase.PreUpdate, 4);
        world.AddSystem(new BlockSystem(), SystemExecutionPhase.PreUpdate, 5);
        world.AddSystem(new AttackSystem(), SystemExecutionPhase.PreUpdate, 6);
        world.AddSystem(new MoveSystem(), SystemExecutionPhase.PreUpdate, 7);
        world.AddSystem(new JumpSystem(), SystemExecutionPhase.PreUpdate, 8);
        world.AddSystem(new AirControlSystem(), SystemExecutionPhase.PreUpdate, 9);
        world.AddSystem(new ProjectileShootingSystem(), SystemExecutionPhase.PreUpdate, 10);
        world.AddSystem(new ItemSwitchSystem(), SystemExecutionPhase.PreUpdate, 11);
        world.AddSystem(new ObjectSwitchSystem(), SystemExecutionPhase.PreUpdate, 13);
        world.AddSystem(new DamageSystem(), SystemExecutionPhase.PreUpdate, 12);
    }

    private static void AddUpdateSystems(World world)
    {
        // Update Phase - Core physics simulation
        world.AddSystem(new GravitySystem(), SystemExecutionPhase.Update, 1);
        world.AddSystem(new FrictionSystem(), SystemExecutionPhase.Update, 2);
        world.AddSystem(new AirResistanceSystem(), SystemExecutionPhase.Update, 3);
        world.AddSystem(new ForceSystem(), SystemExecutionPhase.Update, 4);
        world.AddSystem(new VelocitySystem(), SystemExecutionPhase.Update, 5);
        world.AddSystem(new PositionSystem(), SystemExecutionPhase.Update, 6);
    }

    private static void AddPostUpdateSystems(World world, GameAssets assets)
    {
        // PostUpdate Phase - Collision resolution and state updates
        world.AddSystem(new CollisionDetectionSystem(), SystemExecutionPhase.PostUpdate, 1);
        world.AddSystem(new CollisionResponseSystem(), SystemExecutionPhase.PostUpdate, 2);
        world.AddSystem(new GroundedSystem(), SystemExecutionPhase.PostUpdate, 3);
        world.AddSystem(new PlayerStateSystem(), SystemExecutionPhase.PostUpdate, 4);
        world.AddSystem(new FacingSystem(), SystemExecutionPhase.PostUpdate, 5);
        world.AddSystem(new AnimationSystem(), SystemExecutionPhase.PostUpdate, 6);
        world.AddSystem(new ProjectileSpawningSystem(assets), SystemExecutionPhase.PostUpdate, 7);
        world.AddSystem(new CharacterSwitchSystem(assets), SystemExecutionPhase.PreUpdate, 8);
        world.AddSystem(new DespawnSystem(), SystemExecutionPhase.PostUpdate, 9);

        //world.AddSystem(new ActionDebugSystem(), SystemExecutionPhase.PostUpdate, 6);
    }

    private static void AddRenderSystems(World world, GameAssets assets, GraphicsManager graphicsManager)
    {
        // Add base render system
        world.AddSystem(new RenderSystem(graphicsManager.spriteBatch), SystemExecutionPhase.Render, 0);
        world.AddSystem(new UIRenderSystem(assets, graphicsManager.spriteBatch), SystemExecutionPhase.Render, 1);

        // Add the debug render system
        world.AddSystem(new DebugRenderSystem(assets, graphicsManager), SystemExecutionPhase.Render, 1);
    }
}