using ECS.Components.Animation;
using ECS.Components.Input;
using ECS.Components.Characters;
using ECS.Resources;
using ECS.Components.Timer;
using ECS.Components.Collision;

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
        var worldParam  = Expression.Parameter(typeof(World),   MAGIC.UTILS.WORLD);
        var entityParam = Expression.Parameter(typeof(Entity), MAGIC.UTILS.ENTITY);
        var objParam    = Expression.Parameter(typeof(object), MAGIC.UTILS.VALUE);

        // Get the World.GetPool<T>() method using reflection, then make it generic for componentType (so we can use <T>).
        MethodInfo getPoolMethod = typeof(World)
            .GetMethod(MAGIC.UTILS.GETPOOL)
            .MakeGenericMethod(componentType);

        // Build an expression that represents calling world.GetPool<T>()
        var getPoolCall = Expression.Call(worldParam, getPoolMethod);

        // Convert Value object to the actual component type T
        var castComponent = Expression.Convert(objParam, componentType);

        // Create a generic version of ComponentPool<componentType> (Again so we can use <T>)
        var poolType = typeof(ComponentPool<>).MakeGenericType(componentType);

        // Get its Set method using reflection
        MethodInfo setMethod = poolType.GetMethod(MAGIC.UTILS.SET);

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

            // Special handling for Timers: deep clone the TimerMap so each entity gets its own instance 	ECS.dll!ECS.Core.EntityFactory.CreateEntityFromKey(string entityKey, ECS.Core.GameAssets assets) Line 47	C#

            if (componentType == typeof(Timers))
            {
                // Assume componentValue is of type Timers
                var configTimers = (Timers)componentValue;
                var newTimerMap = new Dictionary<TimerType, Timer>(configTimers.TimerMap);
                var newTimers = new Timers { TimerMap = newTimerMap };
                setter(world, entity, newTimers);
            }
            else if(componentType == typeof(CollisionBody)) {
                var body = (CollisionBody)componentValue;
                var newPolygons = new List<Polygon>(body.Polygons);
                var newBody = new CollisionBody { Polygons = newPolygons };
                setter(world, entity, newBody);
            }
            else
            {
                // Use cached setter for all other components
                setter(world, entity, componentValue);
            }
        }
    }

    public static void ApplySpriteAndAnimation(World world, Entity entity, Texture2D spriteSheet, AnimationConfig animationConfig)
    {
        if (spriteSheet == null || animationConfig.Equals(default(AnimationConfig)) ||
            animationConfig.States == null || animationConfig.States.Count == 0)
        {
            return;
        }

        string animationState = animationConfig.States.Keys.FirstOrDefault() ?? MAGIC.ANIMATIONSTATE.IDLE;

        if (animationConfig.States.ContainsKey(animationState))
        {
            var firstFrame = animationConfig.States[animationState][0].SourceRect;
            var drawLayer = DrawLayer.Terrain;
            if (world.GetPool<SpriteConfig>().Has(entity))
            {
                drawLayer = world.GetPool<SpriteConfig>().Get(entity).Layer;
            }
            world.GetPool<SpriteConfig>().Set(entity, new SpriteConfig
            {
                Texture = spriteSheet,
                SourceRect = firstFrame,
                Origin = new Vector2(firstFrame.Width / 2, firstFrame.Height / 2),
                Color = Color.White,
                Layer = drawLayer
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