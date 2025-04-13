namespace ECS.Core;

/// <summary>
/// Manages camera state and transformations for the game
/// </summary>
public class CameraManager
{
    private readonly GraphicsDevice graphicsDevice;

    // Camera properties
    private Vector2 position;
    private float zoom = 1.0f;
    private Matrix transformMatrix = Matrix.Identity;
    
    // Original reference size
    private Point referenceResolution;

    // Camera settings
    private const float DEFAULT_ZOOM = 1.0f;
    private const float MIN_ZOOM = 0.5f;
    private const float MAX_ZOOM = 3.0f;
    private const float ZOOM_SPEED = 0.1f;
    private const float LERP_VALUE = 0.1f;
    
    // Added flag to control scaling behavior
    private bool maintainWorldScale = true;

    public CameraManager(GraphicsDevice graphicsDevice)
    {
        this.graphicsDevice = graphicsDevice;
        
        // Store reference resolution for calculations
        referenceResolution = new Point(800, 600);
        
        Reset();
    }
    
    /// <summary>
    /// Handle window resize events
    /// </summary>
    public void HandleResize(Point newSize)
    {
        // Update the matrix when the window is resized
        UpdateMatrix();
    }

    /// <summary>
    /// Updates the camera's transform matrix based on its current position and zoom level
    /// </summary>
    private void UpdateMatrix()
    {
        // Get the viewport dimensions
        var viewportWidth = graphicsDevice.Viewport.Width;
        var viewportHeight = graphicsDevice.Viewport.Height;
        var screenCenter = new Vector2(viewportWidth / 2f, viewportHeight / 2f);
        
        float finalZoom = zoom;
        
        if (maintainWorldScale)
        {
            // Don't apply any aspect ratio scaling
            // Just use the raw zoom value set by the user
        }
        else
        {
            // Original scaling behavior for UI and other elements that should adjust to screen size
            float currentAspectRatio = (float)viewportWidth / viewportHeight;
            float targetAspectRatio = (float)referenceResolution.X / referenceResolution.Y;
            float scaleRatio = 1.0f;
            
            if (currentAspectRatio > targetAspectRatio)
            {
                // Width is too wide, scale based on height
                scaleRatio = (float)viewportHeight / referenceResolution.Y;
            }
            else
            {
                // Height is too tall, scale based on width
                scaleRatio = (float)viewportWidth / referenceResolution.X;
            }
            
            // Apply base scale ratio to ensure consistent game world scale
            finalZoom = zoom * scaleRatio;
        }

        // The transformation is applied in reverse order:
        // 1. Translate the world so that the camera position is at the origin.
        // 2. Scale the world by the zoom factor.
        // 3. Translate back so that the camera is in the center of the screen.
        transformMatrix =
            Matrix.CreateTranslation(new Vector3(-position, 0)) *
            Matrix.CreateScale(new Vector3(finalZoom, finalZoom, 1)) *
            Matrix.CreateTranslation(new Vector3(screenCenter, 0));
    }

    /// <summary>
    /// Zooms the camera in or out
    /// </summary>
    /// <param name="zoomAmount">Amount to zoom in (positive) or out (negative).</param>
    public void Zoom(float zoomAmount)
    {
        // Apply zoom and clamp to min/max values
        zoom = MathHelper.Clamp(zoom + zoomAmount * ZOOM_SPEED, MIN_ZOOM, MAX_ZOOM);

        // Update the transformation matrix
        UpdateMatrix();
    }
    
    /// <summary>
    /// Toggle whether world scale is maintained during window resizing
    /// </summary>
    public void SetMaintainWorldScale(bool maintain)
    {
        if (maintainWorldScale != maintain)
        {
            maintainWorldScale = maintain;
            UpdateMatrix();
        }
    }

    /// <summary>
    /// Resets the camera to its default state
    /// </summary>
    public void Reset()
    {
        // Reset zoom
        zoom = DEFAULT_ZOOM;

        // Get the viewport dimensions
        var viewportWidth = graphicsDevice.Viewport.Width;
        var viewportHeight = graphicsDevice.Viewport.Height;

        // Reset position to center of screen
        position = new Vector2(viewportWidth / 2f, viewportHeight / 2f);

        // Update the transformation matrix
        UpdateMatrix();
    }

    /// <summary>
    /// Gets the current camera transform matrix
    /// </summary>
    public Matrix GetTransformMatrix()
    {
        return transformMatrix;
    }

    /// <summary>
    /// Gets the current zoom level
    /// </summary>
    public float GetZoom()
    {
        return zoom;
    }
    
    /// <summary>
    /// Gets the effective zoom including window scaling if not maintaining world scale
    /// </summary>
    public float GetEffectiveZoom()
    {
        if (maintainWorldScale)
            return zoom;
            
        var viewportWidth = graphicsDevice.Viewport.Width;
        var viewportHeight = graphicsDevice.Viewport.Height;
        float currentAspectRatio = (float)viewportWidth / viewportHeight;
        float targetAspectRatio = (float)referenceResolution.X / referenceResolution.Y;
        
        // Calculate the effective zoom factor including window scaling
        float scaleRatio;
        if (currentAspectRatio > targetAspectRatio)
        {
            scaleRatio = (float)viewportHeight / referenceResolution.Y;
        }
        else
        {
            scaleRatio = (float)viewportWidth / referenceResolution.X;
        }
        
        return zoom * scaleRatio;
    }

    public void UpdatePosition(Vector2 position)
    {
        this.position = (position * LERP_VALUE + this.position * (1 - LERP_VALUE));
        UpdateMatrix();
    }

    public Vector2 GetPosition()
    {
        return position;
    }
}