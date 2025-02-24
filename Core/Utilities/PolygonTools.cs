using ECS.Components.Collision;
using ECS.Components.Physics;
using ECS.Components.Animation;

namespace ECS.Core.Utilities;

public static class PolygonTools {

    /// <summary>
    /// Calculates the center point of a polygon.
    /// </summary>
    /// <param name="vertices">Array of polygon vertices.</param>
    /// <returns>Center point of the polygon.</returns>
    public static Vector2 GetPolygonCenter(Vector2[] vertices)
    {
        Vector2 center = Vector2.Zero;
        foreach (var vertex in vertices)
        {
            center += vertex;
        }
        return center / vertices.Length;
    }

    /// <summary>
    /// Projects a polygon onto an axis and returns the min/max points of projection.
    /// </summary>
    /// <param name="vertices">Vertices of the polygon to project.</param>
    /// <param name="axis">Axis to project onto.</param>
    /// <returns>Tuple containing min and max points of projection.</returns>
    public static (float X, float Y) ProjectPolygon(Vector2[] vertices, Vector2 axis)
    {
        float min = float.MaxValue;
        float max = float.MinValue;
        foreach (var vertex in vertices)
        {
            float proj = Vector2.Dot(vertex, axis);
            min = Math.Min(min, proj);
            max = Math.Max(max, proj);
        }
        return (min, max);
    }

    /// <summary>
    /// Transforms polygon vertices from local space to world space, accounting for entity scale and position.
    /// </summary>
    /// <param name="entity">The entity the polygon belongs to.</param>
    /// <param name="polygon">The polygon whose vertices to transform.</param>
    /// <param name="pos">The entity's position component.</param>
    /// <param name="scale">The entity's scale component (optional).</param>
    /// <returns>Array of transformed vertices in world space.</returns>
    public static Vector2[] GetTransformedVertices(Entity entity, Polygon polygon, Position pos, Scale scale = default)
    {
        // Ensure the scale is valid. If the default (zero) scale is detected, substitute an identity scale.
        if (scale.Value == Vector2.Zero)
        {
            scale = new Scale { Value = new Vector2(1, 1) };
        }

        Matrix transformMatrix = Matrix.CreateScale(scale.Value.X, scale.Value.Y, 1f) *
                                Matrix.CreateTranslation(pos.Value.X, pos.Value.Y, 0f);

        var transformed = new Vector2[polygon.Vertices.Length];
        for (int i = 0; i < polygon.Vertices.Length; i++)
        {
            transformed[i] = Vector2.Transform(polygon.Vertices[i], transformMatrix);
        }
        return transformed;
    }
}