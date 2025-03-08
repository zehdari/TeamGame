using ECS.Components.Animation;
using ECS.Components.Lives;
using ECS.Components.Physics;
using ECS.Components.Tags;
using ECS.Components.UI;
using ECS.Events;

namespace ECS.Systems.UI;

public class HUDRenderSystem : SystemBase
{
    private readonly SpriteBatch spriteBatch;
    private readonly GameAssets assets;
    private readonly GraphicsManager graphics;
    private List<Entity> renderQueue = new();
    public override bool Pausible => false;

    public HUDRenderSystem(GameAssets gameAssets, GraphicsManager graphicsManager)
    {
        graphics = graphicsManager;
        spriteBatch = graphicsManager.spriteBatch;
        assets = gameAssets;
    }

    public override void Update(World world, GameTime gameTime)
    {
        SpriteConfig HUDSprite = new();
        AnimationConfig hudConfig = new();
        HUDConfig config = new();
        UIText UIConfig = new();
        Scale scale = new Scale();
        TextScale textScale = new();
        // Get the HUD sprite
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<HUDConfig>(entity) || !HasComponents<SpriteConfig>(entity) 
                || !HasComponents<AnimationConfig>(entity) || !HasComponents<UIText>(entity)
                || !HasComponents<Scale>(entity) || !HasComponents<TextScale>(entity))
                continue;

            HUDSprite = GetComponent<SpriteConfig>(entity);
            hudConfig = GetComponent<AnimationConfig>(entity);
            config = GetComponent<HUDConfig>(entity);
            UIConfig = GetComponent<UIText>(entity);
            scale = GetComponent<Scale>(entity);
            textScale = GetComponent<TextScale>(entity);
        }

        // Now render the updated sprites
        renderQueue.Clear();
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<LivesCount>(entity) || !HasComponents<Percent>(entity))
                continue;
            //only render sprites that should be during current pause state
            if (HasComponents<UIPaused>(entity))
            {
                ref var UIPaused = ref GetComponent<UIPaused>(entity);
                if (GameStateHelper.IsPaused(World) != UIPaused.Value)
                    continue;
            }

            renderQueue.Add(entity);
        }

        // Draw entities in sorted order
        var currentPlayer = 0;
        foreach (var entity in renderQueue)
        {
            ref var lives = ref GetComponent<LivesCount>(entity);
            ref var percent = ref GetComponent<Percent>(entity);
            var position = graphics.GetWindowSize();
            var drawPosition = new Vector2(position.X / (renderQueue.Count + 1), (float)(position.Y * .875));
            drawPosition.X *= ++currentPlayer;

            var spriteEffects = SpriteEffects.None;

            float rotation = 0f;

            // Get layer depth from graphics manager
            float layerDepth = graphics.GetLayerDepth(HUDSprite.Layer);

            //get main frame
            if (!hudConfig.States.ContainsKey(config.Frame))
                continue;

            var frames = hudConfig.States[config.Frame];
            HUDSprite.SourceRect = frames[0].SourceRect;

            spriteBatch.Draw(
                HUDSprite.Texture,
                drawPosition,
                HUDSprite.SourceRect,
                HUDSprite.Color,
                rotation,
                HUDSprite.Origin,
                scale.Value,
                spriteEffects,
                layerDepth
            );

            var font = assets.GetFont(UIConfig.Font);
            UIConfig.Text = $"{percent.Value:P0}"; // Special formatting for percents

            spriteBatch.DrawString(
                font,
                UIConfig.Text,
                drawPosition + config.TextPosition,
                UIConfig.Color,
                rotation,
                Vector2.Zero,
                textScale.Value,
                SpriteEffects.None,
                layerDepth
            );

            //get lives frame
            if (!hudConfig.States.ContainsKey(config.Lives))
                continue;

            frames = hudConfig.States[config.Lives];
            HUDSprite.SourceRect = frames[0].SourceRect;

            for (var i = 0; i < lives.Lives; i++)
            {
                spriteBatch.Draw(
                HUDSprite.Texture,
                drawPosition + config.LivesPosition + (config.LivesOffset * i),
                HUDSprite.SourceRect,
                HUDSprite.Color,
                rotation,
                HUDSprite.Origin,
                scale.Value,
                spriteEffects,
                layerDepth
            );
            }
        }
    }
}