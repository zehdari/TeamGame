namespace ECS.Core.Utilities;

public static class SpriteUtils
{
    public static void ApplySpriteAndAnimation(World world, Entity entity, Texture2D spriteSheet, AnimationConfig animationConfig)
    {
        if (spriteSheet == null || animationConfig.Equals(default(AnimationConfig)) ||
            animationConfig.States == null || animationConfig.States.Count == 0)
        {
            return; // No sprite or animation, so no rendering-related components needed
        }

        string animationState = animationConfig.States.Keys.FirstOrDefault() ?? "idle";

        if (animationConfig.States.ContainsKey(animationState))
        {
            var firstFrame = animationConfig.States[animationState][0].SourceRect;

            // Set up SpriteConfig
            world.GetPool<SpriteConfig>().Set(entity, new SpriteConfig
            {
                Texture = spriteSheet,
                SourceRect = firstFrame,
                Origin = new Vector2(firstFrame.Width / 2, firstFrame.Height / 2),
                Color = Color.White,
                Layer = DrawLayer.Terrain
            });

            // Ensure AnimationState exists, otherwise create a default one
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

            // Set AnimationConfig
            world.GetPool<AnimationConfig>().Set(entity, animationConfig);
        }
        else
        {
            Console.WriteLine("WARNING: No valid animation states found in AnimationConfig!");
        }
    }
}