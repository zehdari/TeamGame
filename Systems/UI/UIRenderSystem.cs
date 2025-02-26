using ECS.Components.Animation;
using ECS.Components.UI;
using ECS.Components.Physics;
using System.ComponentModel.Design;

namespace ECS.Systems.UI;

public class UIRenderSystem : SystemBase
{
    private readonly GameAssets assets;
    private readonly SpriteBatch spriteBatch;
    public override bool Pausible => false;

    public UIRenderSystem(GameAssets assets, SpriteBatch spriteBatch)
    {
        this.spriteBatch = spriteBatch;
        this.assets = assets;
    }

    public override void Update(World world, GameTime gameTime)
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<UIText>(entity) || !HasComponents<Position>(entity))
                continue;

            //only render sprites that should be during current pause state
            if (HasComponents<UIPaused>(entity))
            {
                ref var UIPaused = ref GetComponent<UIPaused>(entity);
                if (GameStateHelper.IsPaused(World) != UIPaused.Value)
                    continue;
            }

            ref var Position = ref GetComponent<Position>(entity);
            ref var UIConfig = ref GetComponent<UIText>(entity);

            if (HasComponents<UIMenu>(entity))
            {
                ref var Menu = ref GetComponent<UIMenu>(entity);
                var i = 0;
                foreach (var Button in Menu.Buttons)
                {
                    i += 20;
                    UIText Text = UIConfig;
                    Text.Text = Button.Text;
                    if (Button.Active)
                    {
                        Text.Color = Button.Color;
                    }
                    spriteBatch.DrawString(assets.GetFont(Text.Font), Text.Text, Position.Value + new Vector2(0,i), Text.Color);
                }
            } else
            {
                if (HasComponents<Percent>(entity))
                {
                    ref var percent = ref GetComponent<Percent>(entity);
                    UIConfig.Text = $"{percent.Value:P0}"; // Special formatting for percents
                }
                spriteBatch.DrawString(assets.GetFont(UIConfig.Font), UIConfig.Text, Position.Value, UIConfig.Color);
            }

        }

    }

}