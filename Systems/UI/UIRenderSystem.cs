using ECS.Components.Animation;
using ECS.Components.UI;
using ECS.Components.Physics;

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
            if (!HasComponents<UIConfig>(entity) || !HasComponents<Position>(entity))
                continue;


            ref var UIConfig = ref GetComponent<UIConfig>(entity);
            ref var Position = ref GetComponent<Position>(entity);


            spriteBatch.DrawString(UIConfig.Font, UIConfig.Text, Position.Value, UIConfig.Color);

        }

    }

}