using ECS.Components.Animation;
using ECS.Components.Lives;
using ECS.Components.Physics;
using ECS.Components.Tags;
using ECS.Components.UI;
using ECS.Events;

namespace ECS.Systems.UI;

public class HUDRenderSystem : SystemBase
{
    private const float LAYER_OFFSET = 0.00001f;
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

        // Get the current transformation matrix and temporarily use the identity matrix
        Matrix currentTransform = graphics.GetTransformMatrix();
        
        // Draw entities in sorted order
        const float Y_SCALAR = 0.875f;
        var currentPlayer = 0;
        foreach (var entity in renderQueue)
        {
            ref var lives = ref GetComponent<LivesCount>(entity);
            ref var percent = ref GetComponent<Percent>(entity);
            var screenSize = graphics.GetWindowSize();
            var drawPosition = new Vector2(screenSize.X / (renderQueue.Count + 1), (float)(screenSize.Y * Y_SCALAR));
            drawPosition.X *= ++currentPlayer;

            var spriteEffects = SpriteEffects.None;

            float rotation = 0f;

            // Get base layer depth from graphics manager
            float baseLayerDepth = graphics.GetLayerDepth(HUDSprite.Layer);

            //get main frame
            if (!hudConfig.States.ContainsKey(config.Frame))
                continue;

            var frames = hudConfig.States[config.Frame];
            HUDSprite.SourceRect = frames[0].SourceRect;

            // Draw main frame slightly behind everything else
            float mainFrameDepth = baseLayerDepth;
            
            // Transform the position from screen to world space
            Vector2 worldPos = Vector2.Transform(drawPosition, Matrix.Invert(currentTransform));
            
            spriteBatch.Draw(
                HUDSprite.Texture,
                worldPos,
                HUDSprite.SourceRect,
                HUDSprite.Color,
                rotation,
                HUDSprite.Origin,
                scale.Value,
                spriteEffects,
                mainFrameDepth
            );

            var font = assets.GetFont(UIConfig.Font);
            UIConfig.Text = $"{percent.Value}%"; // Simple percentage

            // Draw text slightly in front of the main frame
            const float DEPTH_OFFSET = 0.0001f;
            float textDepth = baseLayerDepth + DEPTH_OFFSET;
            
            // Transform text position as well
            Vector2 textWorldPos = Vector2.Transform(drawPosition + config.TextPosition, Matrix.Invert(currentTransform));
            
            spriteBatch.DrawString(
                font,
                UIConfig.Text,
                textWorldPos,
                UIConfig.Color,
                rotation,
                Vector2.Zero,
                textScale.Value,
                SpriteEffects.None,
                textDepth
            );

            //get lives frame
            if (!hudConfig.States.ContainsKey(config.Lives))
                continue;

            frames = hudConfig.States[config.Lives];
            HUDSprite.SourceRect = frames[0].SourceRect;

            // Draw each heart with a slightly increasing depth
            // so they never compete for the same z-index
            for (var i = 0; i < lives.Lives; i++)
            {
                // Calculate a unique depth for each heart
                // Small increment per heart to ensure consistent ordering
                float heartDepth = baseLayerDepth + LAYER_OFFSET + (i * LAYER_OFFSET);
                
                // Transform heart position
                Vector2 heartWorldPos = Vector2.Transform(
                    drawPosition + config.LivesPosition + (config.LivesOffset * i), 
                    Matrix.Invert(currentTransform)
                );
                
                spriteBatch.Draw(
                    HUDSprite.Texture,
                    heartWorldPos,
                    HUDSprite.SourceRect,
                    HUDSprite.Color,
                    rotation,
                    HUDSprite.Origin,
                    scale.Value,
                    spriteEffects,
                    heartDepth
                );
            }
        }
    }
}