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
        InputConfig inputConfig = default,
        Vector2 position = default
        )
    {
        var entity = CreateEntityFromConfig(config, spriteSheet, animationConfig, inputConfig);
        
        world.GetPool<PlayerTag>().Set(entity, new PlayerTag());

        var characterConfig = world.GetPool<CharacterConfig>().Get(entity);

        ref var positionComponent = ref world.GetPool<Position>().Get(entity);
        positionComponent.Value = position;
        
        // Reinitialize character config
        EntityUtils.InitializeCharacterConfig(world, entity);
        
        return entity;
    }

    public Entity CreateAIFromConfig(
        EntityConfig config,
        Texture2D spriteSheet = null,
        AnimationConfig animationConfig = default,
        Vector2 position = default
        )
    {
        var entity = CreateEntityFromConfig(config, spriteSheet, animationConfig);
        
        world.GetPool<AITag>().Set(entity, new AITag());
        var characterConfig = world.GetPool<CharacterConfig>().Get(entity);

        ref var positionComponent = ref world.GetPool<Position>().Get(entity);
        positionComponent.Value = position;

        
        // Reinitialize character config
        EntityUtils.InitializeCharacterConfig(world, entity);
        
        return entity;
    }

    public Entity CreateProjectileFromConfig(
        EntityConfig config,
        Texture2D spriteSheet = null,
        AnimationConfig animationConfig = default,
        Vector2 position = default,
        bool isFacingLeft = default
        )
    {
        var entity = CreateEntityFromConfig(config, spriteSheet, animationConfig);

        world.GetPool<ProjectileTag>().Set(entity, new ProjectileTag());

       ref var velocity = ref world.GetPool<Velocity>().Get(entity);
       ref var positionComponent = ref world.GetPool<Position>().Get(entity);

        // Set projectile specific stuff
        positionComponent.Value = position;
        velocity.Value.X = isFacingLeft ? -Math.Abs(velocity.Value.X) : Math.Abs(velocity.Value.X);

        return entity;
    }

    public Entity CreateHitboxFromConfig(
        EntityConfig config,
        Vector2 position = default
        )
    {
        var entity = CreateEntityFromConfig(config, null, default);

        // This sets the top left corner of the hitbox rectangle, with the dimensions being 
        // determined by config
        world.GetPool<Position>().Set(entity, new Position
        {
            Value = position
        });

        return entity;
    }


    public Entity CreateLine(Vector2 start, Vector2 end, float thickness = 1.0f)
    {
        var entity = world.CreateEntity();

        world.GetPool<Position>().Set(entity, new Position 
        { 
            Value = Vector2.Zero
        });

        // Ensure thickness is at least 1
        thickness = Math.Max(thickness, 1.0f);

        // Compute direction from start to end, normalized
        Vector2 direction = end - start;
        direction.Normalize();

        // Compute the perpendicular (rotated 90°)
        Vector2 perpendicular = new Vector2(-direction.Y, direction.X);

        // Offset by half the thickness
        float halfThickness = thickness / 2f;
        Vector2 offset = perpendicular * halfThickness;

        // Define the rectangle (quadrilateral) vertices
        // Order vertices to form a proper polygon
        Vector2[] vertices = new Vector2[]
        {
            start - offset, // bottom-left
            start + offset, // top-left
            end + offset,   // top-right
            end - offset    // bottom-right
        };

        world.GetPool<CollisionBody>().Set(entity, new CollisionBody
        {
            Polygons = new List<Polygon>
            {
                new Polygon
                {
                    Vertices = vertices,
                    IsTrigger = false,
                    Layer = CollisionLayer.World,
                    CollidesWith = CollisionLayer.Physics | CollisionLayer.World
                }
            },
        });

        return entity;
    }


    public void CreateWorldBoundaries(int screenWidth, int screenHeight)
    {
        // Floor
        CreateLine(new Vector2(0, screenHeight), new Vector2(screenWidth, screenHeight));

        // Left wall
        CreateLine(new Vector2(0, 0), new Vector2(0, screenHeight));

        // Right wall
        CreateLine(new Vector2(screenWidth, 0), new Vector2(screenWidth, screenHeight));

        // Ceiling
        CreateLine(new Vector2(0, 0), new Vector2(screenWidth, 0));
    }
}