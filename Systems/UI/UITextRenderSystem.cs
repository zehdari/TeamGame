using ECS.Components.Animation;
using ECS.Components.UI;
using ECS.Components.Physics;
using ECS.Components.Tags;
using ECS.Core.Utilities;
using Microsoft.Xna.Framework.Graphics;
using static System.Net.Mime.MediaTypeNames;

namespace ECS.Systems.UI;

public class UITextRenderSystem : SystemBase
{
    private readonly GameAssets assets;
    private readonly GraphicsManager graphics;
    private readonly SpriteBatch spriteBatch;
    
    public override bool Pausible => false;

    public UITextRenderSystem(GameAssets assets, GraphicsManager graphicsManager)
    {
        this.graphics = graphicsManager;
        this.spriteBatch = graphicsManager.spriteBatch;
        this.assets = assets;
    }
    
    private static Vector2 CenterText(SpriteFont font, string text, TextCenter center)
    {
        var measurement = font.MeasureString(text) / 2;
        return measurement * center.Value;
    }

    public override void Update(World world, GameTime gameTime)
    {
        // Get camera zoom for counter-scaling
        Matrix cameraMatrix = graphics.cameraManager.GetTransformMatrix();
        float cameraZoom = graphics.cameraManager.GetZoom();
        
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<UIText>(entity))
                continue;

            // Only render sprites that should be during current pause state
            if (HasComponents<UIPaused>(entity))
            {
                ref var UIPaused = ref GetComponent<UIPaused>(entity);
                if (GameStateHelper.IsPaused(World) != UIPaused.Value)
                    continue;
            }

            if (HasComponents<MainMenuTag>(entity))
            {
                // Skip rendering if not in main menu state
                if (!GameStateHelper.IsMenu(World))
                    continue;
            }

            if (HasComponents<LevelSelectTag>(entity))
            {
                // Skip rendering if not in level select state
                if (!GameStateHelper.IsLevelSelect(World))
                    continue;
            }

            Vector2 drawPosition;
            
            if (HasComponents<UIPosition>(entity))
            {
                ref var Position = ref GetComponent<UIPosition>(entity);
                var windowSize = graphics.GetWindowSize();
                // Convert UI coordinates (0-1) to screen coordinates
                Vector2 screenPos = new Vector2(Position.Value.X * windowSize.X, Position.Value.Y * windowSize.Y);
                // Convert screen coordinates to world coordinates for drawing
                drawPosition = Vector2.Transform(screenPos, Matrix.Invert(cameraMatrix));
            }
            else if (HasComponents<Position>(entity))
            {
                drawPosition = GetComponent<Position>(entity).Value;
            }
            else 
            {
                // Skip entities without position
                continue;
            }

            ref var UIConfig = ref GetComponent<UIText>(entity);
            var font = assets.GetFont(UIConfig.Font);
            
            // Get layer depth - UI text should use DrawLayer.UIText for depth
            float layerDepth = graphics.GetLayerDepth(DrawLayer.UIText);
            
            if (HasComponents<Percent>(entity))
            {
                ref var percent = ref GetComponent<Percent>(entity);
                UIConfig.Text = $"{percent.Value:P0}"; // Special formatting for percents
            }

            

            TextCenter center;
            if (HasComponents<TextCenter>(entity))
            {
                center = GetComponent<TextCenter>(entity);
            } else
            {
                center = new();
            }
            
            // Apply counter-scaling to keep UI text consistent size
            Vector2 scale = Vector2.One / cameraZoom;
            if (HasComponents<TextScale>(entity))
            {
                ref var scaleComponent = ref GetComponent<TextScale>(entity);
                scale = scaleComponent.Value / cameraZoom;
            }

            float rotation = 0f;
            if (HasComponents<Rotation>(entity))
            {
                ref var rotationComponent = ref GetComponent<Rotation>(entity);
                rotation = rotationComponent.Value;
            }

            var centeredPosition = CenterText(font, UIConfig.Text, center);
            spriteBatch.DrawString(
                font,
                UIConfig.Text,
                drawPosition,
                UIConfig.Color,
                rotation,
                centeredPosition,
                scale,
                SpriteEffects.None,
                layerDepth
            );

            if (HasComponents<UIMenu2D>(entity))
            {
                ref var Menu2D = ref GetComponent<UIMenu2D>(entity);
                Vector2 currentPosition = drawPosition;

                foreach (var Menu in Menu2D.Menus)
                {
                    foreach (var Button in Menu.Buttons)
                    {
                        UIText Text = UIConfig;
                        Text.Text = Button.Text;
                        if (Button.Active)
                        {
                            Text.Color = Button.Color;
                        }
                        centeredPosition = CenterText(font, Text.Text, center);
                        spriteBatch.DrawString(
                            font,
                            Text.Text,
                            currentPosition,
                            Text.Color,
                            rotation,
                            centeredPosition,
                            scale,
                            SpriteEffects.None,
                            layerDepth
                        );
                        // Scale menu separation to maintain fixed-size appearance
                        currentPosition.Y += Menu.Separation / cameraZoom;
                    }
                    currentPosition.Y = drawPosition.Y;
                    currentPosition.X += Menu2D.Separation / cameraZoom;
                }
            }
            else
            if (HasComponents<UIMenu>(entity))
            {
                ref var Menu = ref GetComponent<UIMenu>(entity);
                Vector2 currentPosition = drawPosition;
                
                foreach (var Button in Menu.Buttons)
                {
                    UIText Text = UIConfig;
                    Text.Text = Button.Text;
                    if (Button.Active)
                    {
                        Text.Color = Button.Color;
                    }
                    centeredPosition = CenterText(font, Text.Text, center);
                    spriteBatch.DrawString(
                        font, 
                        Text.Text, 
                        currentPosition, 
                        Text.Color,
                        rotation,
                        centeredPosition,
                        scale,
                        SpriteEffects.None,
                        layerDepth
                    );
                    // Scale menu separation to maintain fixed-size appearance
                    currentPosition.Y += Menu.Separation / cameraZoom;
                }
            }
        }
    }
}