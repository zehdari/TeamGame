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
using ECS.Resources;
using ECS.Components.UI;
using ECS.Components.Lives;

namespace ECS.Core;

public class EntityFactory
{
    private readonly World world;
    private string[] PortMagic = { MAGIC.GAMEPAD.PLAYER_ONE, MAGIC.GAMEPAD.PLAYER_TWO, MAGIC.GAMEPAD.PLAYER_THREE, MAGIC.GAMEPAD.PLAYER_FOUR };

    public EntityFactory(World world)
    {
        this.world = world;
    }

    public Entity CreateGameStateEntity(GameAssets assets) {
        return CreateEntityFromKey(MAGIC.ASSETKEY.GAMESTATE, assets);
    }

    public Entity CreateEntityFromKey(string entityKey, GameAssets assets)
    {
        // Get entity assets from registry
        var assetKeys = EntityRegistry.GetEntity(entityKey);
        if (assetKeys == null)
        {
            throw new ArgumentException($"Entity with key '{entityKey}' not found in registry");
        }

        // Load assets
        var config = assets.GetEntityConfig(assetKeys.ConfigKey);
        var sprite = string.IsNullOrEmpty(assetKeys.SpriteKey) ? null : assets.GetTexture(assetKeys.SpriteKey);
        var animation = string.IsNullOrEmpty(assetKeys.AnimationKey) ? default : assets.GetAnimation(assetKeys.AnimationKey);
        var input = string.IsNullOrEmpty(assetKeys.InputKey) ? default : assets.GetInputConfig(assetKeys.InputKey);

        // Create entity with loaded assets
        return CreateEntityFromConfig(config, sprite, animation, input);
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
        var portPool = world.GetPool<OpenPorts>();
        int count = 0;

        foreach (var character in world.GetEntities())
        {

            if (!world.GetPool<OpenPorts>().Has(character)) continue;
            // increment count if OpenPorts.port != "AcceptsAll"
            ref var portComponentTemp = ref world.GetPool<OpenPorts>().Get(character);
            if(portComponentTemp.port != MAGIC.GAMEPAD.ACCEPTS_ALL) count++;
            
        }

        ref var portComponent = ref world.GetPool<OpenPorts>().Get(entity);
        portComponent.port = PortMagic[count-1];

        positionComponent.Value = position;

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
}