using ECS.Components.Animation;
using ECS.Components.Input;
using ECS.Resources;

namespace ECS.Core.Utilities;

public static class EntityUtils 
{
    // Cache for component setters (So we only have to pay for reflection once)
    private static readonly Dictionary<Type, Action<World, Entity, object>> componentSetterCache = new();

    private static Action<World, Entity, object> CreateSetter(Type componentType)
    {
        var worldType = typeof(World);
        var poolMethod = worldType.GetMethod("GetPool")?.MakeGenericMethod(componentType);
        
        return (world, entity, value) => {
            var pool = poolMethod?.Invoke(world, null);
            // Get the Set method from the actual pool instance
            var setMethod = pool?.GetType().GetMethod("Set");
            if (pool != null && setMethod != null)
            {
                setMethod.Invoke(pool, new[] { entity, value });
            }
            else
            {
                Console.WriteLine($"Failed to set component of type {componentType.Name}");
            }
        };
    }

  
    public static void ApplyComponents(World world, Entity entity, EntityConfig config)
    {
        foreach (var componentEntry in config.Components)
        {
            var componentType = componentEntry.Key;
            var componentValue = componentEntry.Value;

            // Get or create setter
            if (!componentSetterCache.TryGetValue(componentType, out var setter))
            {
                setter = CreateSetter(componentType);
                componentSetterCache[componentType] = setter;
            }

            // Use cached setter
            setter(world, entity, componentValue);
        }
    }

    public static void ApplySpriteAndAnimation(World world, Entity entity, Texture2D spriteSheet, AnimationConfig animationConfig)
    {
        if (spriteSheet == null || animationConfig.Equals(default(AnimationConfig)) ||
            animationConfig.States == null || animationConfig.States.Count == 0)
        {
            return;
        }

        string animationState = animationConfig.States.Keys.FirstOrDefault() ?? "idle";

        if (animationConfig.States.ContainsKey(animationState))
        {
            var firstFrame = animationConfig.States[animationState][0].SourceRect;

            world.GetPool<SpriteConfig>().Set(entity, new SpriteConfig
            {
                Texture = spriteSheet,
                SourceRect = firstFrame,
                Origin = new Vector2(firstFrame.Width / 2, firstFrame.Height / 2),
                Color = Color.White,
                Layer = DrawLayer.Terrain
            });

            if (!world.GetPool<AnimationState>().Has(entity))
            {
                world.GetPool<AnimationState>().Set(entity, new AnimationState
                {
                    CurrentState = animationState,
                    TimeInFrame = 0,
                    FrameIndex = 0,
                    IsPlaying = true
                });
            }

            world.GetPool<AnimationConfig>().Set(entity, animationConfig);
        }
        else
        {
            Console.WriteLine("WARNING: No valid animation states found in AnimationConfig!");
        }
    }

    public static void ApplyInputConfig(World world, Entity entity, InputConfig inputConfig)
    {
        if (!inputConfig.Equals(default(InputConfig)) && inputConfig.Actions != null && inputConfig.Actions.Count > 0)
        {
            world.GetPool<InputConfig>().Set(entity, inputConfig);
        }
    }
}