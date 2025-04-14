using ECS.Components.Animation;
using ECS.Components.Physics;
using ECS.Components.UI;
using ECS.Components.Tags;
using ECS.Core.Utilities;
using ECS.Core;
using Microsoft.Xna.Framework.Graphics;
using static System.Formats.Asn1.AsnWriter;

namespace ECS.Systems.Animation;

public class RenderSystem : SystemBase
{
    private readonly GraphicsManager graphicsManager;
    private readonly SpriteBatch spriteBatch;
    private List<Entity> renderQueue = new();
    
    public override bool Pausible => false;

    public RenderSystem(GraphicsManager graphicsManager)
    {
        this.graphicsManager = graphicsManager;
        this.spriteBatch = graphicsManager.spriteBatch;
    }

    public override void Update(World world, GameTime gameTime)
    {
        // Get inverse of camera transform for UI elements
        Matrix cameraMatrix = graphicsManager.cameraManager.GetTransformMatrix();
        float cameraZoom = graphicsManager.cameraManager.GetZoom();
        
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
                // Skip rendering if not in main menu state
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
            
            // Handle UI elements differently - screen-space positioning and counter-scaling
            bool isUIElement = sprite.Layer == DrawLayer.UI || 
                              HasComponents<UIText>(entity) || 
                              HasComponents<UIMenu>(entity);
            
            if (isUIElement)
            {
                // For UI elements, convert to screen space
                if (HasComponents<UIPosition>(entity))
                {
                    // Convert UI coordinates (0-1) to screen coordinates
                    ref var uiPosition = ref GetComponent<UIPosition>(entity);
                    var windowSize = graphicsManager.GetWindowSize();
                    Vector2 screenPos = new Vector2(
                        uiPosition.Value.X * windowSize.X, 
                        uiPosition.Value.Y * windowSize.Y
                    );
                    
                    // Convert screen position to world space for rendering
                    drawPosition = Vector2.Transform(screenPos, Matrix.Invert(cameraMatrix));
                }
                
                // Counter-scale UI elements - make them appear the same size regardless of zoom
                if (HasComponents<Scale>(entity))
                {
                    ref var scaleComponent = ref GetComponent<Scale>(entity);
                    scale = scaleComponent.Value / cameraZoom; // Divide by zoom to counter-scale
                }
                else
                {
                    scale = Vector2.One / cameraZoom; // Counter-scale by default for UI
                }
            }
            else
            {
                // For world elements, use normal scaling
                if (HasComponents<Scale>(entity))
                {
                    ref var scaleComponent = ref GetComponent<Scale>(entity);
                    scale = scaleComponent.Value;
                }
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

                foreach (var Menu in Menu2D.Menus) { 
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
                        // Adjust separation based on zoom for UI elements
                        menuPosition.Y += Menu.Separation / (isUIElement ? cameraZoom : 1.0f);
                    }
                    menuPosition.Y = drawPosition.Y;
                    menuPosition.X += Menu2D.Separation / (isUIElement ? cameraZoom : 1.0f);
                }
                //Draw yellow outline for Character and Level menus
                if (HasComponents<ButtonSelected>(entity) && HasComponents<UIMenu>(entity) && UniqueButtons)
                {
                    ref var buttonSelected = ref GetComponent<ButtonSelected>(entity);
                    ref var Menu = ref GetComponent<UIMenu>(entity);
                    AnimationFrameConfig[] SourceRect;
                    menuPosition.X = drawPosition.X + Menu2D.Selected * Menu2D.Separation;
                    menuPosition.Y = drawPosition.Y + Menu.Selected * Menu.Separation;
                    if (ButtonSprites.TryGetValue(buttonSelected.Value, out SourceRect)) { }
                    spriteBatch.Draw(
                        sprite.Texture,
                        menuPosition,
                        SourceRect[0].SourceRect,
                        sprite.Color,
                        rotation,
                        sprite.Origin,
                        scale,
                        spriteEffects,
                        graphicsManager.GetLayerDepth(DrawLayer.UIOverlay1)
                    );
                }
                //Draw Player Indicators for Character menu
                if (HasComponents<PlayerIndicators>(entity) && HasComponents<PlayerCount>(entity) && UniqueButtons)
                {
                    ref var indicators = ref GetComponent<PlayerIndicators>(entity);
                    ref var playerCount = ref GetComponent<PlayerCount>(entity);
                    //Update position of currently selecting player before drawing
                    if (playerCount.Value < playerCount.MaxValue)
                    {
                        ref var indicator = ref indicators.Values[playerCount.Value];
                        indicator.Position = menuPosition;
                        indicator.Value = indicator.Value == -1 ? 0 : indicator.Value;
                    }
                    foreach (var indicator in indicators.Values)
                    {
                        AnimationFrameConfig[] SourceRect;
                        menuPosition = indicator.Position + indicator.Offset;
                        if (indicator.Value >= 0 && ButtonSprites.TryGetValue(indicator.PotentialValues[indicator.Value], out SourceRect))
                        {
                            spriteBatch.Draw(
                                sprite.Texture,
                                menuPosition,
                                SourceRect[0].SourceRect,
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
            } else
            if (HasComponents<UIMenu>(entity))
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
                    // Adjust separation based on zoom for UI elements
                    menuPosition.Y += Menu.Separation / (isUIElement ? cameraZoom : 1.0f);
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

    /*private void DrawMenu(Entity entity, Vector2 menuPosition, SpriteConfig sprite, float rotation, Vector2 scale, SpriteEffects spriteEffects, float layerDepth)
    {
        ref var Menu = ref GetComponent<UIMenu>(entity);

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
            // Adjust separation based on zoom for UI elements
            menuPosition.Y += Menu.Separation / (isUIElement ? cameraZoom : 1.0f);
        }
    }*/
}