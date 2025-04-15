using ECS.Components.Animation;
using ECS.Components.Physics;
using ECS.Components.Tags;
using ECS.Components.Camera;

namespace ECS.Systems.Camera;

public class CameraSystem : SystemBase
{
    private readonly CameraManager cameraManager;
    private bool previousCameraTrackingEnabled;
    
    public CameraSystem(CameraManager cameraManager)
    {
        this.cameraManager = cameraManager;
    }
    
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<ActionEvent>(HandleCameraAction);
        previousCameraTrackingEnabled = false;
    }
    
    private void HandleCameraAction(IEvent evt)
    {
        var actionEvent = (ActionEvent)evt;
        
        // Only process actions when they're active
        if (!actionEvent.IsHeld)
            return;
            
        switch (actionEvent.ActionName)
        {
            case "camera_zoom_in":
                cameraManager.Zoom(1.0f);
                break;
                
            case "camera_zoom_out":
                cameraManager.Zoom(-1.0f);
                break;
                
            case "camera_reset":
                cameraManager.Reset();
                break;
        }
    }
    
    public override void Update(World world, GameTime gameTime) 
    {
        // Check if camera tracking is enabled on the game state entity
        bool cameraTrackingEnabled = false; // Default to disabled
        
        foreach (var entity in World.GetEntities())
        {
            if (HasComponents<SingletonTag>(entity) && HasComponents<CameraTracking>(entity))
            {
                ref var tracking = ref GetComponent<CameraTracking>(entity);
                cameraTrackingEnabled = tracking.Value;
                break;
            }
        }
        
        // Only track entities if camera tracking is enabled
        if (cameraTrackingEnabled)
        {
            var playerNum = 0;
            var newPosition = Vector2.Zero;
            
            foreach (var entity in World.GetEntities())
            {
                if ((!HasComponents<PlayerTag>(entity) && !HasComponents<AITag>(entity)) || 
                    !HasComponents<Position>(entity))
                    continue;

                ref var position = ref GetComponent<Position>(entity);
                newPosition += position.Value;
                playerNum++;
            }
            
            if (playerNum > 0)
            {
                newPosition /= playerNum;
                cameraManager.UpdatePosition(newPosition);
            }
            else
            {
                cameraManager.Reset();
            }
        }
        if (cameraTrackingEnabled != previousCameraTrackingEnabled)
        {
            if (!cameraTrackingEnabled)
            {
                cameraManager.Reset();
            }
            previousCameraTrackingEnabled = cameraTrackingEnabled;
            
        }
    }
}