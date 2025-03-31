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

    // Camera settings
    private const float DEFAULT_ZOOM = 1.0f;
    private const float MIN_ZOOM = 0.5f;
    private const float MAX_ZOOM = 3.0f;
    private const float ZOOM_SPEED = 0.1f;
    private const float LERP_VALUE = 0.1f;

    public CameraManager(GraphicsDevice graphicsDevice)
    {
        this.graphicsDevice = graphicsDevice;
        Reset();
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

        // The transformation is applied in reverse order:
        // 1. Translate the world so that the camera position is at the origin.
        // 2. Scale the world by the zoom factor.
        // 3. Translate back so that the camera is in the center of the screen.
        transformMatrix =
            Matrix.CreateTranslation(new Vector3(-position, 0)) *
            Matrix.CreateScale(new Vector3(zoom, zoom, 1)) *
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