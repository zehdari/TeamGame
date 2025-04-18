using ECS.Components.Animation;
using ECS.Components.Physics;
using ECS.Components.UI;
using ECS.Components.Tags;
using ECS.Core.Utilities;

namespace ECS.Systems.Animation;

public class RenderSystem : SystemBase
{
    private readonly GraphicsManager graphicsManager;
    private readonly SpriteBatch spriteBatch;
    private List<Entity> renderQueue = new();
    
    public override bool Pausible => false;
    public override bool UseScaledGameTime => false;

    public RenderSystem(GraphicsManager graphicsManager)
    {
        this.graphicsManager = graphicsManager;
        this.spriteBatch = graphicsManager.spriteBatch;
    }

    public override void Update(World world, GameTime gameTime)
    {
        // Get camera transform
        Matrix cameraMatrix = graphicsManager.cameraManager.GetTransformMatrix();
        Point windowSize = graphicsManager.GetWindowSize();
        Point referenceSize = new Point(800, 600); // Original reference size
        
        // Calculate adaptive UI scale for screen-space UI elements
        float scaleX = windowSize.X / (float)referenceSize.X;
        float scaleY = windowSize.Y / (float)referenceSize.Y;
        float uiScale = Math.Min(scaleX, scaleY);
        
        // Ensure all animations are updated before rendering
        foreach (var entity in World.GetEntities())
        {
            if (HasComponents<AnimationState>(entity) && 
                HasComponents<SpriteConfig>(entity) && 
                HasComponents<AnimationConfig>(entity))
            {
                ref var state = ref GetComponent<AnimationState>(entity);
                ref var sprite = ref GetComponent<SpriteConfig>(entity);
                ref var config = ref GetComponent<AnimationConfig>(entity);

                if (!state.IsPlaying || !config.States.ContainsKey(state.CurrentState))
                    continue;

                var frames = config.States[state.CurrentState];
                sprite.SourceRect = frames[state.FrameIndex].SourceRect;
            }
        }

        // Get entities for rendering
        renderQueue.Clear();
        
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<Position>(entity) || !HasComponents<SpriteConfig>(entity))
                continue;
                
            // Skip entities not appropriate for current pause state
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
                // Skip rendering if not in main menu state
                if (!GameStateHelper.IsCharacterSelect(World))
                    continue;
            }

            renderQueue.Add(entity);
        }

        // Render all entities
        foreach (var entity in renderQueue)
        {
            ref var position = ref GetComponent<Position>(entity);
            ref var sprite = ref GetComponent<SpriteConfig>(entity);

            // Get layer depth from graphics manager
            float layerDepth = graphicsManager.GetLayerDepth(sprite.Layer);

            Vector2 drawPosition = position.Value;
            Vector2 scale = Vector2.One;
            
            // Determine if this is a UI element
            bool isScreenSpaceUI = sprite.Layer == DrawLayer.UI || 
                                  HasComponents<UIText>(entity) || 
                                  HasComponents<UIMenu>(entity) ||
                                  HasComponents<UIPosition>(entity);
            
            if (isScreenSpaceUI)
            {
                if (HasComponents<UIPosition>(entity))
                {
                    // Convert UI coordinates (0-1) to screen coordinates
                    ref var uiPosition = ref GetComponent<UIPosition>(entity);
                    Vector2 screenPos = new Vector2(
                        uiPosition.Value.X * windowSize.X, 
                        uiPosition.Value.Y * windowSize.Y
                    );
                    
                    // Convert screen position to world space for rendering
                    drawPosition = Vector2.Transform(screenPos, Matrix.Invert(cameraMatrix));
                }
                
                // Draw UI sprites at their native size (no scaling up)
                if (HasComponents<Scale>(entity))
                {
                    ref var scaleComponent = ref GetComponent<Scale>(entity);
                    scale = scaleComponent.Value;
                }
                else
                {
                    scale = Vector2.One;
                }
            }
            else
            {
                // For world elements, use normal scaling from the Scale component
                if (HasComponents<Scale>(entity))
                {
                    ref var scaleComponent = ref GetComponent<Scale>(entity);
                    scale = scaleComponent.Value;
                }
            }

            if (HasComponents<Parallax>(entity))
            {
                ref var parallax = ref GetComponent<Parallax>(entity);
                var camPos = graphicsManager.cameraManager.GetPosition();
                drawPosition = position.Value * parallax.Value + camPos * (Vector2.One - parallax.Value);
            }

            var spriteEffects = SpriteEffects.None;
            if (HasComponents<FacingDirection>(entity))
            {
                ref var facing = ref GetComponent<FacingDirection>(entity);
                spriteEffects = facing.IsFacingLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            }

            float rotation = 0f;
            if (HasComponents<Rotation>(entity))
            {
                ref var rotationComponent = ref GetComponent<Rotation>(entity);
                rotation = rotationComponent.Value;
            }

            if (HasComponents<UIMenu2D>(entity))
            {
                ref var Menu2D = ref GetComponent<UIMenu2D>(entity);
                Vector2 menuPosition = drawPosition;
                var UniqueButtons = HasComponents<LevelSelectTag>(entity) || HasComponents<CharacterSelectTag>(entity);
                Dictionary<String, AnimationFrameConfig[]> ButtonSprites = new();
                if (UniqueButtons)
                {
                    ref var config = ref GetComponent<AnimationConfig>(entity);
                    ButtonSprites = config.States;
                }
                foreach (var Menu in Menu2D.Menus) 
                { 
                    foreach (var Button in Menu.Buttons)
                    {
                        spriteBatch.Draw(
                            sprite.Texture,
                            menuPosition,
                            sprite.SourceRect,
                            sprite.Color,
                            rotation,
                            sprite.Origin,
                            scale,
                            spriteEffects,
                            layerDepth
                        );
                        
                        if (UniqueButtons)
                        {
                            AnimationFrameConfig[] SourceRect;
                            if (ButtonSprites.TryGetValue(Button.Action, out SourceRect)) { 
                                spriteBatch.Draw(
                                    sprite.Texture,
                                    menuPosition,
                                    SourceRect[0].SourceRect,
                                    sprite.Color,
                                    rotation,
                                    sprite.Origin,
                                    scale,
                                    spriteEffects,
                                    graphicsManager.GetLayerDepth(DrawLayer.UIOverlay2)
                                );
                            }
                        }
                        // Use raw pixel separation
                        menuPosition.Y += Menu.Separation;
                    }
                    menuPosition.Y = drawPosition.Y;
                    // Use raw pixel separation for columns
                    menuPosition.X += Menu2D.Separation;
                }
                // Draw yellow outline for Character and Level menus
                if (HasComponents<ButtonSelected>(entity) 
                    && HasComponents<UIMenu>(entity) 
                    && UniqueButtons)
                {
                    ref var buttonSelected = ref GetComponent<ButtonSelected>(entity);
                    ref var menu       = ref GetComponent<UIMenu>(entity);

                    // raw separations
                    float sepX = Menu2D.Separation;
                    float sepY = menu.Separation;

                    Vector2 selPos = drawPosition;
                    selPos.X += Menu2D.Selected * sepX;
                    selPos.Y += menu.Selected    * sepY;

                    if (ButtonSprites.TryGetValue(buttonSelected.Value, out var sourceRect))
                    {
                        spriteBatch.Draw(
                            sprite.Texture,
                            selPos,
                            sourceRect[0].SourceRect,
                            sprite.Color,
                            rotation,
                            sprite.Origin,
                            scale,
                            spriteEffects,
                            graphicsManager.GetLayerDepth(DrawLayer.UIOverlay1)
                        );
                    }
                }

                // Draw Player Indicators for Character menu
                if (HasComponents<PlayerIndicators>(entity) && HasComponents<PlayerCount>(entity) && UniqueButtons)
                {
                    ref var indicators = ref GetComponent<PlayerIndicators>(entity);
                    ref var playerCount = ref GetComponent<PlayerCount>(entity);
                    ref var menu       = ref GetComponent<UIMenu>(entity);
                    ref var menu2D     = ref GetComponent<UIMenu2D>(entity);

                    // raw separations
                    float sepX = menu2D.Separation;
                    float sepY = menu.Separation;

                    // compute base position of the selected slot
                    Vector2 basePos = drawPosition
                                    + new Vector2(menu2D.Selected * sepX,
                                                  menu.Selected   * sepY);

                    // keep indicator.Position in sync
                    if (playerCount.Value < playerCount.MaxValue)
                    {
                        ref var current = ref indicators.Values[playerCount.Value];
                        current.Position = basePos;
                        current.Value    = current.Value == -1 ? 0 : current.Value;
                    }

                    foreach (var indicator in indicators.Values)
                    {
                        if (indicator.Value < 0)
                            continue;

                        if (!ButtonSprites.TryGetValue(
                                indicator.PotentialValues[indicator.Value], 
                                out var sourceRects))
                            continue;

                        // apply raw offset
                        Vector2 drawPos = indicator.Position + indicator.Offset;

                        spriteBatch.Draw(
                            sprite.Texture,
                            drawPos,
                            sourceRects[0].SourceRect,
                            sprite.Color,
                            rotation,
                            sprite.Origin,
                            scale,
                            spriteEffects,
                            graphicsManager.GetLayerDepth(DrawLayer.UIOverlay3)
                        );
                    }
                }
            } 
            else if (HasComponents<UIMenu>(entity))
            {
                ref var Menu = ref GetComponent<UIMenu>(entity);
                Vector2 menuPosition = drawPosition;
                
                foreach (var Button in Menu.Buttons)
                {
                    spriteBatch.Draw(
                        sprite.Texture,
                        menuPosition,
                        sprite.SourceRect,
                        sprite.Color,
                        rotation,
                        sprite.Origin,
                        scale,
                        spriteEffects,
                        layerDepth
                    );
                    
                    // Use raw pixel separation
                    menuPosition.Y += Menu.Separation;
                }
            }
            else
            {
                spriteBatch.Draw(
                    sprite.Texture,
                    drawPosition,
                    sprite.SourceRect,
                    sprite.Color,
                    rotation,
                    sprite.Origin,
                    scale,
                    spriteEffects,
                    layerDepth
                );
            }
        }
    }
}
