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
using ECS.Systems.Lives;
using ECS.Systems.Player;
using ECS.Systems.Attacking;
using ECS.Systems.Spawning;
using ECS.Systems.Hitbox;
using ECS.Systems.Camera;
using ECS.Systems.Sound;
using ECS.Systems.Damage;
using ECS.Systems.Blocking;

namespace ECS.Core;

public static class SystemBuilder
{

    public static void BuildSystems(World world, GameStateManager gameStateManager, GameAssets assets, GraphicsManager graphicsManager, LevelLoader levelLoader, SoundManager soundManager)
    {
        AddInputSystems(world);
        AddPreUpdateSystems(world, gameStateManager, assets, levelLoader);
        AddUpdateSystems(world);
        AddPostUpdateSystems(world, gameStateManager, assets, graphicsManager, soundManager);
        AddRenderSystems(world, assets, graphicsManager);
        AddTerminalSystem(world, assets, graphicsManager);
    }

    private static void AddInputSystems(World world)
    {
        // Input Phase - Handle raw input and generate events
        world.AddSystem(new RawInputSystem(), SystemExecutionPhase.Input, 1);
        world.AddSystem(new InputMappingSystem(), SystemExecutionPhase.Input, 2);
        world.AddSystem(new GamePadDebugSystem(), SystemExecutionPhase.Input, 3);
    }

    private static void AddPreUpdateSystems(World world, GameStateManager gameStateManager, GameAssets assets, LevelLoader levelLoader)
    {
        // PreUpdate Phase - Handle input events and generate forces
        world.AddSystem(new GameStateSystem(gameStateManager), SystemExecutionPhase.PreUpdate, 0);
        world.AddSystem(new MenuSystem(gameStateManager), SystemExecutionPhase.PreUpdate, 1);
        world.AddSystem(new LevelLoaderSystem(gameStateManager, levelLoader), SystemExecutionPhase.PreUpdate, 1);
        world.AddSystem(new RandomSystem(), SystemExecutionPhase.PreUpdate, 1);
        world.AddSystem(new TimerSystem(), SystemExecutionPhase.PreUpdate, 2);
        world.AddSystem(new AISystem(), SystemExecutionPhase.PreUpdate, 3);
        world.AddSystem(new BlockRegenerationSystem(), SystemExecutionPhase.PreUpdate, 4);
        world.AddSystem(new BlockSystem(), SystemExecutionPhase.PreUpdate, 4);
        world.AddSystem(new BlockActionSystem(), SystemExecutionPhase.PreUpdate, 5);
        world.AddSystem(new AttackSystem(), SystemExecutionPhase.PreUpdate, 6);
        world.AddSystem(new MoveSystem(), SystemExecutionPhase.PreUpdate, 7);
        world.AddSystem(new JumpSystem(), SystemExecutionPhase.PreUpdate, 8);
        world.AddSystem(new AirControlSystem(), SystemExecutionPhase.PreUpdate, 9);
        world.AddSystem(new ProjectileShootingSystem(), SystemExecutionPhase.PreUpdate, 10);
        world.AddSystem(new ItemSwitchSystem(), SystemExecutionPhase.PreUpdate, 11);
        world.AddSystem(new ObjectSwitchSystem(), SystemExecutionPhase.PreUpdate, 13);
        world.AddSystem(new DamageSystem(), SystemExecutionPhase.PreUpdate, 12);
        world.AddSystem(new HitResolutionSystem(), SystemExecutionPhase.PreUpdate, 13);
        world.AddSystem(new DropThroughSystem(), SystemExecutionPhase.PreUpdate, 14);
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

    private static void AddPostUpdateSystems(World world, GameStateManager gameStateManager, GameAssets assets, GraphicsManager graphicsManager, SoundManager soundManager)
    {
        // PostUpdate Phase - Collision resolution and state updates
        world.AddSystem(new CollisionDetectionSystem(), SystemExecutionPhase.PostUpdate, 1);
        world.AddSystem(new CollisionResponseSystem(), SystemExecutionPhase.PostUpdate, 2);

        world.AddSystem(new PlayerDespawnSystem(graphicsManager), SystemExecutionPhase.PostUpdate, 3);
        world.AddSystem(new GroundedSystem(), SystemExecutionPhase.PostUpdate, 3);
        world.AddSystem(new HitDetectionSystem(), SystemExecutionPhase.PostUpdate, 3);
        world.AddSystem(new ProjectileHitSystem(), SystemExecutionPhase.PostUpdate, 3);
        world.AddSystem(new AttackHitSystem(), SystemExecutionPhase.PostUpdate, 3);
        world.AddSystem(new LivesSystem(), SystemExecutionPhase.PostUpdate, 4);
        world.AddSystem(new PlayerStateSystem(), SystemExecutionPhase.PostUpdate, 4);
        world.AddSystem(new FacingSystem(), SystemExecutionPhase.PostUpdate, 4);
        world.AddSystem(new AnimationSystem(), SystemExecutionPhase.PostUpdate, 5);
        world.AddSystem(new PlayerSpawningSystem(), SystemExecutionPhase.PostUpdate, 5);
        world.AddSystem(new HitboxSpawningSystem(assets), SystemExecutionPhase.PostUpdate, 5);
        world.AddSystem(new LevelSwitchSystem(gameStateManager), SystemExecutionPhase.PostUpdate, 9);
        world.AddSystem(new ProjectileDespawnSystem(), SystemExecutionPhase.PostUpdate, 10);
        world.AddSystem(new SplatPeaSpawningSystem(assets), SystemExecutionPhase.PostUpdate, 11);
        world.AddSystem(new HitboxDespawnSystem(), SystemExecutionPhase.PostUpdate, 11);
        world.AddSystem(new ProjectileSpawningSystem(assets), SystemExecutionPhase.PostUpdate, 11);
        world.AddSystem(new CharacterSwitchSystem(assets), SystemExecutionPhase.PreUpdate, 12);
        world.AddSystem(new DespawnSystem(), SystemExecutionPhase.PostUpdate, 13);
        world.AddSystem(new SoundSystem(soundManager), SystemExecutionPhase.PostUpdate, 14);
    }

    private static void AddRenderSystems(World world, GameAssets assets, GraphicsManager graphicsManager)
    {
        // Add base render system
        world.AddSystem(new CameraSystem(graphicsManager.cameraManager), SystemExecutionPhase.PreUpdate, 0);
        world.AddSystem(new UIPositionSystem(graphicsManager), SystemExecutionPhase.Render, 1);
        world.AddSystem(new RenderSystem(graphicsManager), SystemExecutionPhase.Render, 2);
        world.AddSystem(new HUDRenderSystem(assets, graphicsManager), SystemExecutionPhase.Render, 3);
        world.AddSystem(new UITextRenderSystem(assets, graphicsManager), SystemExecutionPhase.Render, 4);
        world.AddSystem(new DebugRenderSystem(assets, graphicsManager), SystemExecutionPhase.Render, 4);
    }

    private static void AddTerminalSystem(World world, GameAssets assets, GraphicsManager graphicsManager)
    {
        world.AddSystem(new TerminalSystem(assets, graphicsManager), SystemExecutionPhase.Terminal, 0);
    }
}