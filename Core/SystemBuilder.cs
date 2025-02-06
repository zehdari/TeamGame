using ECS.Systems.AI;
using ECS.Systems.Animation;
using ECS.Systems.Collision;
using ECS.Systems.Input;
using ECS.Systems.Physics;
using ECS.Systems.Projectile;
using ECS.Systems.State;
using ECS.Systems.Utilities;

namespace ECS.Core;

public static class SystemBuilder
{
    public static void BuildCoreSystems(World world, EntityFactory entityFactory)
    {
        AddInputSystems(world);
        AddPreUpdateSystems(world);
        AddUpdateSystems(world);
        AddPostUpdateSystems(world, entityFactory);
    }

    public static void BuildRenderSystems(World world, SpriteBatch spriteBatch, SpriteFont debugFont = null)
    {
        AddRenderSystems(world, spriteBatch, debugFont);
    }

    private static void AddInputSystems(World world)
    {
        // Input Phase - Handle raw input and generate events
        world.AddSystem(new RawInputSystem(), SystemExecutionPhase.Input, 1);
        world.AddSystem(new InputMappingSystem(), SystemExecutionPhase.Input, 2);
    }

    private static void AddPreUpdateSystems(World world)
    {
        // PreUpdate Phase - Handle input events and generate forces
        world.AddSystem(new RandomSystem(), SystemExecutionPhase.PreUpdate, 1);
        world.AddSystem(new TimerSystem(), SystemExecutionPhase.PreUpdate, 2);
        world.AddSystem(new AISystem(), SystemExecutionPhase.PreUpdate, 3);
        world.AddSystem(new ProjectileSystem(), SystemExecutionPhase.PreUpdate, 3);
        world.AddSystem(new JumpSystem(), SystemExecutionPhase.PreUpdate, 3);
        world.AddSystem(new MoveSystem(), SystemExecutionPhase.PreUpdate, 4);
        world.AddSystem(new AirControlSystem(), SystemExecutionPhase.PreUpdate, 4);
        world.AddSystem(new ProjectileShootingSystem(), SystemExecutionPhase.PreUpdate, 5);
    }

    private static void AddUpdateSystems(World world)
    {
        // Update Phase - Core physics simulation
        world.AddSystem(new JumpSystem(), SystemExecutionPhase.Update, 1);
        world.AddSystem(new GravitySystem(), SystemExecutionPhase.Update, 1);
        world.AddSystem(new FrictionSystem(), SystemExecutionPhase.Update, 2);
        world.AddSystem(new AirResistanceSystem(), SystemExecutionPhase.Update, 3);
        world.AddSystem(new ForceSystem(), SystemExecutionPhase.Update, 4);
        world.AddSystem(new VelocitySystem(), SystemExecutionPhase.Update, 5);
        world.AddSystem(new PositionSystem(), SystemExecutionPhase.Update, 6);
    }

    private static void AddPostUpdateSystems(World world, EntityFactory entityFactory)
    {
        // PostUpdate Phase - Collision resolution and state updates
        world.AddSystem(new CollisionDetectionSystem(), SystemExecutionPhase.PostUpdate, 1);
        world.AddSystem(new CollisionResponseSystem(), SystemExecutionPhase.PostUpdate, 2);
        world.AddSystem(new PlayerStateSystem(), SystemExecutionPhase.PostUpdate, 3);
        world.AddSystem(new FacingSystem(), SystemExecutionPhase.PostUpdate, 3);
        world.AddSystem(new AnimationSystem(), SystemExecutionPhase.PostUpdate, 4);
        world.AddSystem(new ProjectileSpawningSystem(entityFactory), SystemExecutionPhase.PostUpdate, 5);
    }

    private static void AddRenderSystems(World world, SpriteBatch spriteBatch, SpriteFont debugFont)
    {
        // Add base render system
        world.AddSystem(new RenderSystem(spriteBatch), SystemExecutionPhase.Render, 0);

        // if (debugFont != null)
        // {
        //     world.AddSystem(new DebugRenderSystem(spriteBatch, GraphicsDevice, debugFont), 
        //         SystemExecutionPhase.Render, 1);
        // }
    }
}