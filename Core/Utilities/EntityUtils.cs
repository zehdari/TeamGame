using ECS.Components.Animation;
using ECS.Components.Input;
using ECS.Resources;

namespace ECS.Core.Utilities;

public static class EntityUtils 
{
    // Cache for component setters (So we only have to pay for reflection once)
    private static readonly Dictionary<Type, Action<World, Entity, object>> setterCache = new();

    private static Action<World, Entity, object> CreateSetter(Type componentType)
    {
        // This creates a delegate like this:
        // (World world, Entity entity, object value) => world.GetPool<T>().Set(entity, (T)value);

        // Sidenote: Expression trees are wild, big fan btw
        
        // Define the parameters for the expression
        var worldParam  = Expression.Parameter(typeof(World),   "world");
        var entityParam = Expression.Parameter(typeof(Entity),  "entity");
        var objParam    = Expression.Parameter(typeof(object),  "value");

        // Get the World.GetPool<T>() method using reflection, then make it generic for componentType (so we can use <T>).
        MethodInfo getPoolMethod = typeof(World)
            .GetMethod("GetPool")
            .MakeGenericMethod(componentType);

        // Build an expression that represents calling world.GetPool<T>()
        var getPoolCall = Expression.Call(worldParam, getPoolMethod);

        // Convert Value object to the actual component type T
        var castComponent = Expression.Convert(objParam, componentType);

        // Create a generic version of ComponentPool<componentType> (Again so we can use <T>)
        var poolType = typeof(ComponentPool<>).MakeGenericType(componentType);

        // Get its Set method using reflection
        MethodInfo setMethod = poolType.GetMethod("Set");

        // Now the expression becomes world.GetPool<T>().Set(entityParam, castComponent)
        var callSet = Expression.Call(getPoolCall, setMethod, entityParam, castComponent);

        // Finally build the lambda expression: (World world, Entity entity, object value) => world.GetPool<T>().Set(entity, (T)value);
        var lambda = Expression.Lambda<Action<World, Entity, object>>(
            callSet,
            worldParam,
            entityParam,
            objParam
        );

        // Compile the delegate ONCE and return
        return lambda.Compile();

        // Now we can call the setter for the component type whenever we want and not have to pay for any reflection 😎💰
    }
  
    public static void ApplyComponents(World world, Entity entity, EntityConfig config)
    {
        foreach (var componentEntry in config.Components)
        {
            var componentType = componentEntry.Key;
            var componentValue = componentEntry.Value;

            // Get or create setter (Only created once per component type)
            if (!setterCache.TryGetValue(componentType, out var setter))
            {
                setter = CreateSetter(componentType);
                setterCache[componentType] = setter;
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