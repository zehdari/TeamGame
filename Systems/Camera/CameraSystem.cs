namespace ECS.Systems.Camera;

public class CameraSystem : SystemBase
{
    private readonly CameraManager cameraManager;
    
    public CameraSystem(CameraManager cameraManager)
    {
        this.cameraManager = cameraManager;
    }
    
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<ActionEvent>(HandleCameraAction);
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
    
    public override void Update(World world, GameTime gameTime) { }
}