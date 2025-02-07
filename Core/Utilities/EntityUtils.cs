namespace ECS.Core.Utilities;

public static class EntityUtils 
{
    public static void ApplyComponents(World world, Entity entity, EntityConfig config)
    {
        foreach (var componentEntry in config.Components)
        {
            var componentType = componentEntry.Key;
            var componentValue = componentEntry.Value;

            var poolMethod = world.GetType()
                .GetMethod("GetPool")
                ?.MakeGenericMethod(componentType)
                .Invoke(world, null);

            var setMethod = poolMethod?.GetType().GetMethod("Set");
            setMethod?.Invoke(poolMethod, new[] { entity, componentValue });
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