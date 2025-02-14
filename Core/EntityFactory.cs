using ECS.Components.AI;
using ECS.Components.Animation;
using ECS.Components.Collision;
using ECS.Components.Input;
using ECS.Components.Physics;
using ECS.Components.Random;
using ECS.Components.State;
using ECS.Components.Tags;
using ECS.Components.Timer;
using ECS.Components.Items;
using ECS.Components.Characters;
using ECS.Core.Utilities;
using ECS.Resources;
using ECS.Components.UI;

namespace ECS.Core;

public class EntityFactory
{
    private readonly World world;

    public EntityFactory(World world)
    {
        this.world = world;
    }

    public Entity CreateGameStateEntity()
    {
        var entity = world.CreateEntity();
        
        world.GetPool<GameStateComponent>().Set(entity, new GameStateComponent 
        { 
            CurrentState = GameState.Running 
        });
        world.GetPool<SingletonTag>().Set(entity, new SingletonTag());

        return entity;
    }

    public Entity CreateEntityFromConfig(
        EntityConfig config,
        Texture2D spriteSheet = null,
        AnimationConfig animationConfig = default,
        InputConfig inputConfig = default)
    {
        var entity = world.CreateEntity();

        // Apply entity components from config
        EntityUtils.ApplyComponents(world, entity, config);

        // Apply sprite and animation components if applicable
        EntityUtils.ApplySpriteAndAnimation(world, entity, spriteSheet, animationConfig);

        // Apply input configuration if available
        EntityUtils.ApplyInputConfig(world, entity, inputConfig);

        return entity;
    }

    public Entity CreatePlayerFromConfig(
        EntityConfig config,
        Texture2D spriteSheet = null,
        AnimationConfig animationConfig = default,
        InputConfig inputConfig = default
        )
    {
        var entity = CreateEntityFromConfig(config, spriteSheet, animationConfig, inputConfig);
        
        world.GetPool<PlayerTag>().Set(entity, new PlayerTag());

        var characterConfig = world.GetPool<CharacterConfig>().Get(entity);
        
        // Reinitialize character config
        EntityUtils.InitializeCharacterConfig(world, entity);
        
        return entity;
    }

    public Entity CreateAIFromConfig(
        EntityConfig config,
        Texture2D spriteSheet = null,
        AnimationConfig animationConfig = default
        )
    {
        var entity = CreateEntityFromConfig(config, spriteSheet, animationConfig);
        
        world.GetPool<AITag>().Set(entity, new AITag());
        var characterConfig = world.GetPool<CharacterConfig>().Get(entity);
        
        // Reinitialize character config
        EntityUtils.InitializeCharacterConfig(world, entity);
        
        return entity;
    }

    public Entity CreateLine(Vector2 start, Vector2 end)
    {
        var entity = world.CreateEntity();

        world.GetPool<Position>().Set(entity, new Position 
        { 
            Value = start 
        });
        world.GetPool<CollisionShape>().Set(entity, new CollisionShape 
        {
            Type = ShapeType.Line,
            Size = end - start,
            Offset = Vector2.Zero,
            IsPhysical = true,
            IsOneWay = false
        });
        world.GetPool<CollisionState>().Set(entity, new CollisionState 
        {
            Sides = CollisionFlags.None,
            CollidingWith = new HashSet<Entity>()
        });

        return entity;
    }

    public Entity CreateProjectile(Texture2D spriteSheet, AnimationConfig animConfig, Vector2 pos, bool isFacingLeft)
    {
        var entity = world.CreateEntity();

        // Core projectile components
        world.GetPool<ProjectileTag>().Set(entity, new ProjectileTag { });
        world.GetPool<ExistedTooLong>().Set(entity, new ExistedTooLong 
        { 
            Value = false 
        });
        world.GetPool<Timer>().Set(entity, new Timer 
        { 
            Duration = 1f, 
            Elapsed = 0f 
        });

        // Physics components
        world.GetPool<Mass>().Set(entity, new Mass 
        { 
            Value = 1f 
        });
        world.GetPool<Position>().Set(entity, new Position 
        { 
            Value = pos 
        });
        world.GetPool<Rotation>().Set(entity, new Rotation 
        { 
            Value = 0f 
        });
        world.GetPool<Scale>().Set(entity, new Scale 
        { 
            Value = Vector2.One 
        });
        world.GetPool<Velocity>().Set(entity, new Velocity 
        { 
            Value = new Vector2(isFacingLeft ? -500 : 500, 0) 
        });
        world.GetPool<MaxVelocity>().Set(entity, new MaxVelocity 
        { 
            Value = 4000f 
        });

        // Visual components
        var sourceRect = animConfig.States["idle"][0].SourceRect;
        world.GetPool<SpriteConfig>().Set(entity, new SpriteConfig 
        {
            Texture = spriteSheet,
            SourceRect = sourceRect,
            Origin = new Vector2(sourceRect.Width / 2, sourceRect.Height / 2),
            Color = Color.White,
            Layer = DrawLayer.Projectile
        });
        world.GetPool<AnimationConfig>().Set(entity, animConfig);
        world.GetPool<AnimationState>().Set(entity, new AnimationState 
        {
            CurrentState = "idle",
            TimeInFrame = 0,
            FrameIndex = 0,
            IsPlaying = true
        });
        world.GetPool<FacingDirection>().Set(entity, new FacingDirection 
        { 
            IsFacingLeft = false 
        });

        return entity;
    }

    public Entity CreateUIText(InputConfig UIInputConfig)
    {
        
        var entity = world.CreateEntity();

        world.GetPool<UIConfig>().Set(entity, new UIConfig
        {
            Font = "DebugFont",
            Text = "0%",
            Color = Color.White
        });

        world.GetPool<Percent>().Set(entity, new Percent
        {
            Value = 0f
        });

        world.GetPool<Position>().Set(entity, new Position
        {
            Value = new Vector2(100, 100)
        });

       
        EntityUtils.ApplyInputConfig(world, entity, UIInputConfig);


        return entity;

    }

    public void CreateWorldBoundaries(EntityFactory entityFactory, int screenWidth, int screenHeight)
    {
        // Floor
        entityFactory.CreateLine(
            new Vector2(0, screenHeight), 
            new Vector2(screenWidth, screenHeight)
        ); 

        // Left wall
        entityFactory.CreateLine(
            new Vector2(0, 0), 
            new Vector2(0, screenHeight)
        ); 

        // Right wall
        entityFactory.CreateLine(
            new Vector2(screenWidth, 0), 
            new Vector2(screenWidth, screenHeight)
        );

        // Ceiling
        entityFactory.CreateLine(
            new Vector2(0, 0), 
            new Vector2(screenWidth, 0)
        );
    }
}