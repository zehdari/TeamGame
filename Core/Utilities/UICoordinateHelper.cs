namespace ECS.Core.Utilities;

/// <summary>
/// Helper class for transforming between screen space and world space for UI elements
/// </summary>
public static class UICoordinateHelper
{
    /// <summary>
    /// Transforms screen coordinates to world coordinates using the inverse camera matrix
    /// </summary>
    public static Vector2 ScreenToWorld(Vector2 screenPosition, CameraManager cameraManager)
    {
        Matrix invertedCamera = Matrix.Invert(cameraManager.GetTransformMatrix());
        return Vector2.Transform(screenPosition, invertedCamera);
    }
    
    /// <summary>
    /// Transforms world coordinates to screen coordinates using the camera matrix
    /// </summary>
    public static Vector2 WorldToScreen(Vector2 worldPosition, CameraManager cameraManager)
    {
        return Vector2.Transform(worldPosition, cameraManager.GetTransformMatrix());
    }
}