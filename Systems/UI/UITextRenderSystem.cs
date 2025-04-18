using ECS.Components.Animation;
using ECS.Components.UI;
using ECS.Components.Physics;
using ECS.Components.Tags;
using ECS.Core.Utilities;

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
        // Get camera transform for UI positioning
        Matrix cameraMatrix = graphics.cameraManager.GetTransformMatrix();
        
        // For UI elements, we still want to scale based on viewport size
        Point windowSize = graphics.GetWindowSize();
        Point referenceSize = new Point(800, 600); // Original reference size
        
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

            if (HasComponents<CharacterSelectTag>(entity))
            {
                // Skip rendering if not in level select state
                if (!GameStateHelper.IsCharacterSelect(World))
                    continue;
            }

            Vector2 drawPosition;
            bool isScreenSpaceUI = HasComponents<UIPosition>(entity);
            
            if (isScreenSpaceUI)
            {
                ref var Position = ref GetComponent<UIPosition>(entity);
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
            }
            else
            {
                center = new();
            }
            
            // Calculate scale for the text
            Vector2 scale = Vector2.One;
            if (HasComponents<TextScale>(entity))
            {
                ref var scaleComponent = ref GetComponent<TextScale>(entity);
                scale = scaleComponent.Value;
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
                        
                        // Use raw pixel separation
                        currentPosition.Y += Menu.Separation;
                    }
                    
                    currentPosition.Y = drawPosition.Y;
                    // Use raw pixel separation for columns
                    currentPosition.X += Menu2D.Separation;
                }
            }
            else if (HasComponents<UIMenu>(entity))
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
                    
                    // Use raw pixel separation
                    currentPosition.Y += Menu.Separation;
                }
            }
        }
    }
}
