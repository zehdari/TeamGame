using ECS.Components.Characters;
using ECS.Components.Animation;
using ECS.Components.Tags;
using ECS.Core.Utilities;
using ECS.Components.Physics;

namespace ECS.Systems.Characters;
public class CharacterSwitchSystem : SystemBase
{
    private readonly GameAssets assets;
    private EntityFactory factory;
    private readonly Dictionary<string, int> actionDirections = new()
    {
        ["switch_character_forward"] = +1,
        ["switch_character_backward"] = -1
    };

    private readonly Queue<Entity> switchQueue = new();
    private int lastDirection;
    private HashSet<string> playableCharacters;

    public CharacterSwitchSystem(GameAssets assets)
    {
        this.assets = assets;
    }

    public override void Initialize(World world)
    {
        base.Initialize(world);
        this.factory = world.entityFactory;
        this.playableCharacters = new HashSet<string>();
        Subscribe<ActionEvent>(HandleCharacterSwitchAction);
        InitializePlayableCharacters();
    }

    private void InitializePlayableCharacters()
    {
        playableCharacters.Clear();
        
        foreach (var character in EntityRegistry.GetEntities())
        {
            var config = assets.GetEntityConfig(character.Value.ConfigKey);
            if (config != null && config.Components.ContainsKey(typeof(CharacterConfig)))
            {
                playableCharacters.Add(character.Key);
            }
        }
    }

    private void HandleCharacterSwitchAction(IEvent evt)
    {
        var actionEvent = (ActionEvent)evt;

        if (GameStateHelper.IsPaused(World))
            return;

        if (!actionEvent.IsStarted)
            return;

        if (!actionDirections.TryGetValue(actionEvent.ActionName, out int direction))
            return;

        lastDirection = direction;

        // Queue all AI entities for switching
        foreach (var entity in World.GetEntities())
        {
            if (HasComponents<AITag>(entity) && 
                HasComponents<CharacterConfig>(entity) &&
                HasComponents<SpriteConfig>(entity) &&
                HasComponents<AnimationConfig>(entity))
            {
                switchQueue.Enqueue(entity);
            }
        }
    }

    public override void Update(World world, GameTime gameTime)
    {
        while (switchQueue.Count > 0)
        {
            var entity = switchQueue.Dequeue();
            
            // Ensure components exist
            if (!HasComponents<CharacterConfig>(entity) || 
                !HasComponents<Position>(entity) || 
                !HasComponents<IsGrounded>(entity))
                continue;

            // Store Position and IsGrounded
            var storedPosition = GetComponent<Position>(entity);
            var storedVelocity = GetComponent<Velocity>(entity);
            var storedIsGrounded = GetComponent<IsGrounded>(entity);

            // Get the list of characters in the registry
            var characters = EntityRegistry.GetEntities()
                .Where(c => playableCharacters.Contains(c.Key))
                .ToList();

            if (characters.Count == 0) continue;

            // Get the current config value (which character the entity currently is)
            ref var config = ref GetComponent<CharacterConfig>(entity);
            var characterConfig = config.Value; // Gotta do this for the lambda

            // Find where the character is in the registry
            int currentIndex = characters.FindIndex(c => c.Key == characterConfig);
            if (currentIndex == -1) continue;

            // Get current character's config to know what components to remove
            var currentCharacterConfig = characters[currentIndex].Value;
            var oldConfig = assets.GetEntityConfig(currentCharacterConfig.ConfigKey);

            // Get the config for the next/previous character
            int nextIndex = (currentIndex + lastDirection + characters.Count) % characters.Count;
            string newCharacter = characters[nextIndex].Key;
            var newCharacterConfig = characters[nextIndex].Value;

            // Load new character from config
            var newConfig = assets.GetEntityConfig(newCharacterConfig.ConfigKey);
            var newSprite = assets.GetTexture(newCharacterConfig.SpriteKey);
            var newAnimConfig = assets.GetAnimation(newCharacterConfig.AnimationKey);

            // Calculate which components need to be removed - ones in old but not in new (Set intersect)
            var componentsToRemove = oldConfig.Components.Keys
                .Except(newConfig.Components.Keys)
                .Where(t => t != typeof(Position) && t != typeof(IsGrounded));

            // Remove only the components that aren't in the new config
            foreach (var componentType in componentsToRemove)
            {
                var getPoolMethod = typeof(World).GetMethod(nameof(World.GetPool)).MakeGenericMethod(componentType);
                var pool = getPoolMethod.Invoke(World, null) as IComponentPool;
                pool?.Remove(entity);
            }

            // Apply new components from the config
            EntityUtils.ApplyComponents(world, entity, newConfig);
            EntityUtils.ApplySpriteAndAnimation(world, entity, newSprite, newAnimConfig);

            // Restore the Position, Velocity and IsGrounded
            World.GetPool<Position>().Set(entity, storedPosition);
            World.GetPool<IsGrounded>().Set(entity, storedIsGrounded);
            World.GetPool<Velocity>().Set(entity, storedVelocity);

            // Update character config to reflect the switch
            World.GetPool<CharacterConfig>().Set(entity, new CharacterConfig { Value = newCharacter });
        }
    }
}
