using ECS.Components.Animation;
using ECS.Components.UI;
using ECS.Components.Physics;
using System.ComponentModel.Design;
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
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<UIPosition>(entity))
                continue;

            //only render sprites that should be during current pause state
            if (HasComponents<UIPaused>(entity))
            {
                ref var UIPaused = ref GetComponent<UIPaused>(entity);
                if (GameStateHelper.IsPaused(World) != UIPaused.Value)
                    continue;
            }

            ref var Position = ref GetComponent<UIPosition>(entity);
            ref var UIConfig = ref GetComponent<UIText>(entity);

            var windowSize = graphics.GetWindowSize();
            Vector2 screenPosition;
            if (HasComponents<Position>(entity))
            {
                screenPosition = GetComponent<Position>(entity).Value;
            }
            else { 
                screenPosition = new Vector2(Position.Value.X * windowSize.X, Position.Value.Y * windowSize.Y);
            }

            var font = assets.GetFont(UIConfig.Font);
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
                screenPosition,
                UIConfig.Color,
                rotation,
                centeredPosition,
                scale,
                SpriteEffects.None,
                0
            );

            if (HasComponents<UIMenu>(entity))
            {
                ref var Menu = ref GetComponent<UIMenu>(entity);
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
                        screenPosition, 
                        Text.Color,
                        rotation,
                        centeredPosition,
                        scale,
                        SpriteEffects.None,
                        0
                    );
                    screenPosition.Y += Menu.Separation;
                }
            }

        }

    }

}