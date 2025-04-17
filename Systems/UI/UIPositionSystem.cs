using ECS.Components.Animation;
using ECS.Components.UI;
using ECS.Components.Physics;
using System.ComponentModel.Design;
using static System.Net.Mime.MediaTypeNames;

namespace ECS.Systems.UI;

public class UIPositionSystem : SystemBase
{
    private readonly GraphicsManager graphics;
    public override bool Pausible => false;

    public UIPositionSystem(GraphicsManager graphicsManager)
    {
        this.graphics = graphicsManager;
    }

    public override void Update(World world, GameTime gameTime)
    {
        var windowSize = graphics.GetWindowSize();

        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<UIPosition>(entity) || !HasComponents<Position>(entity))
                continue;

            ref var uiPosition = ref GetComponent<UIPosition>(entity);
            ref var position = ref GetComponent<Position>(entity);

            position.Value.X = uiPosition.Value.X * windowSize.X;
            position.Value.Y = uiPosition.Value.Y * windowSize.Y;
        }
    }
}