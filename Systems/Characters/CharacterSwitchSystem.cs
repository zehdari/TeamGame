using ECS.Components.Characters;
using ECS.Components.Animation;
using ECS.Components.Tags;
using ECS.Core.Utilities;
using ECS.Components.Physics;
using System.Collections.Generic;
using System.Linq;

namespace ECS.Systems.Characters
{
    public class CharacterSwitchSystem : SystemBase
    {
        private readonly GameAssets assets;
        private readonly EntityFactory factory;
        private readonly Dictionary<string, int> actionDirections = new()
        {
            ["switch_character_forward"] = +1,
            ["switch_character_backward"] = -1
        };

        private readonly Queue<Entity> switchQueue = new();

        public CharacterSwitchSystem(GameAssets assets, EntityFactory factory)
        {
            this.assets = assets;
            this.factory = factory;
        }

        public override void Initialize(World world)
        {
            base.Initialize(world);
            World.EventBus.Subscribe<ActionEvent>(HandleCharacterSwitchAction);
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
                if (!HasComponents<CharacterConfig>(entity) || !HasComponents<Position>(entity) || !HasComponents<IsGrounded>(entity))
                    continue;

                // Store Position and IsGrounded
                var storedPosition = GetComponent<Position>(entity);
                var storedIsGrounded = GetComponent<IsGrounded>(entity);

                // Get the list of characters in the registry
                var characters = CharacterRegistry.GetCharacters().ToList();

                // Get the current config value (which character the entity currently is)
                ref var config = ref GetComponent<CharacterConfig>(entity);
                var characterConfig = config.Value; // Gotta do this for the lambda

                // Find where the character is in the registry
                int currentIndex = characters.FindIndex(c => c.Key == characterConfig);
                if (currentIndex == -1) continue;

                // Get the config for the next/previous character
                int nextIndex = (currentIndex + 1) % characters.Count;
                string newCharacter = characters[nextIndex].Key;
                var newCharacterConfig = characters[nextIndex].Value;

                // Load new character from config
                var newConfig = assets.GetEntityConfig(newCharacterConfig.ConfigKey);
                var newSprite = assets.GetTexture(newCharacterConfig.SpriteKey);
                var newAnimConfig = assets.GetAnimation(newCharacterConfig.AnimationKey);

                // Apply new components from the config
                EntityUtils.ApplyComponents(world, entity, newConfig);
                EntityUtils.ApplySpriteAndAnimation(world, entity, newSprite, newAnimConfig);

                // Restore the Position and IsGrounded
                World.GetPool<Position>().Set(entity, storedPosition);
                World.GetPool<IsGrounded>().Set(entity, storedIsGrounded);

                // Update character config to reflect the switch
                World.GetPool<CharacterConfig>().Set(entity, new CharacterConfig { Value = newCharacter });
            }
        }
    }
}
