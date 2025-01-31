namespace ECS.Systems;

public class CollisionDetectionSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
    }

    public override void Update(World world, GameTime gameTime)
    {
        // Reset collision states
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<CollisionState>(entity)) continue;
            ref var state = ref GetComponent<CollisionState>(entity);
            state.Sides = CollisionFlags.None;
            state.CollidingWith.Clear();
        }

        var colliders = World.GetEntities()
            .Where(e => HasComponents<Position>(e) && 
                       HasComponents<CollisionShape>(e) && 
                       HasComponents<CollisionState>(e))
            .ToList();

        for (int i = 0; i < colliders.Count; i++)
        {
            for (int j = i + 1; j < colliders.Count; j++)
            {
                CheckCollision(colliders[i], colliders[j]);
            }
        }

        // Update grounded states
        foreach (var entity in colliders)
        {
            if (!HasComponents<IsGrounded>(entity)) continue;
            ref var grounded = ref GetComponent<IsGrounded>(entity);
            ref var state = ref GetComponent<CollisionState>(entity);
            
            // Store previous grounded state
            grounded.WasGrounded = grounded.Value;

            // Check current collision with ground
            bool hasGroundContact = (state.Sides & CollisionFlags.Bottom) != 0;

            // Only lose grounded state if we had NO ground contact AND were moving upward
            if (HasComponents<Velocity>(entity))
            {
                ref var vel = ref GetComponent<Velocity>(entity);
                grounded.Value = hasGroundContact || (grounded.WasGrounded && vel.Value.Y >= 0);
            }
            else
            {
                grounded.Value = hasGroundContact || grounded.WasGrounded;
            }
        }
    }

    private void CheckCollision(Entity a, Entity b)
    {
        ref var posA = ref GetComponent<Position>(a);
        ref var shapeA = ref GetComponent<CollisionShape>(a);
        ref var stateA = ref GetComponent<CollisionState>(a);

        ref var posB = ref GetComponent<Position>(b);
        ref var shapeB = ref GetComponent<CollisionShape>(b);
        ref var stateB = ref GetComponent<CollisionState>(b);

        Vector2 normal;
        float penetration;

        bool isColliding;
        if (shapeA.Type == ShapeType.Rectangle && shapeB.Type == ShapeType.Rectangle)
        {
            isColliding = CheckRectangleRectangle(
                posA.Value + shapeA.Offset, shapeA.Size,
                posB.Value + shapeB.Offset, shapeB.Size,
                out normal, out penetration);
        }
        else if (shapeA.Type == ShapeType.Rectangle && shapeB.Type == ShapeType.Line)
        {
            isColliding = CheckRectangleLine(
                posA.Value + shapeA.Offset, shapeA.Size,
                posB.Value + shapeB.Offset, posB.Value + shapeB.Offset + shapeB.Size,
                out normal, out penetration);
        }
        else if (shapeB.Type == ShapeType.Rectangle && shapeA.Type == ShapeType.Line)
        {
            isColliding = CheckRectangleLine(
                posB.Value + shapeB.Offset, shapeB.Size,
                posA.Value + shapeA.Offset, posA.Value + shapeA.Offset + shapeA.Size,
                out normal, out penetration);
            normal = -normal; // Flip normal since we swapped A and B
        }
        else
        {
            return; // Line-line collision not needed
        }

        if (!isColliding) return;

        // Handle one-way platforms
        if (shapeB.IsOneWay && HasComponents<Velocity>(a))
        {
            ref var vel = ref GetComponent<Velocity>(a);
            if (vel.Value.Y < 0) return;

            Vector2 centerA = posA.Value + shapeA.Offset + shapeA.Size / 2;
            Vector2 centerB = posB.Value + shapeB.Offset + shapeB.Size / 2;
            if (centerA.Y > centerB.Y + 4) return;
        }

        // Update collision states
        UpdateCollisionStates(ref stateA, ref stateB, normal);

        // Record colliding pairs
        stateA.CollidingWith.Add(b);
        stateB.CollidingWith.Add(a);

        // Publish collision event
        World.EventBus.Publish(new CollisionEvent
        {
            EntityA = a,
            EntityB = b,
            Normal = normal,
            Penetration = penetration,
            Sides = stateA.Sides
        });
    }

    private bool CheckRectangleRectangle(Vector2 posA, Vector2 sizeA, Vector2 posB, Vector2 sizeB, 
        out Vector2 normal, out float penetration)
    {
        var rectA = new Rectangle((int)posA.X, (int)posA.Y, (int)sizeA.X, (int)sizeA.Y);
        var rectB = new Rectangle((int)posB.X, (int)posB.Y, (int)sizeB.X, (int)sizeB.Y);

        if (!rectA.Intersects(rectB))
        {
            normal = Vector2.Zero;
            penetration = 0;
            return false;
        }

        float overlapX = Math.Min(rectA.Right, rectB.Right) - Math.Max(rectA.Left, rectB.Left);
        float overlapY = Math.Min(rectA.Bottom, rectB.Bottom) - Math.Max(rectA.Top, rectB.Top);

        if (overlapX < overlapY)
        {
            penetration = overlapX;
            normal = new Vector2(rectA.Center.X < rectB.Center.X ? -1 : 1, 0);
        }
        else
        {
            penetration = overlapY;
            normal = new Vector2(0, rectA.Center.Y < rectB.Center.Y ? -1 : 1);
        }

        return true;
    }

    private bool CheckRectangleLine(Vector2 rectPos, Vector2 rectSize, Vector2 lineStart, Vector2 lineEnd,
        out Vector2 normal, out float penetration)
    {
        // Get rectangle corners
        Vector2[] corners = new Vector2[4];
        corners[0] = rectPos; // Top-left
        corners[1] = rectPos + new Vector2(rectSize.X, 0); // Top-right
        corners[2] = rectPos + rectSize; // Bottom-right
        corners[3] = rectPos + new Vector2(0, rectSize.Y); // Bottom-left

        // Line direction and length
        Vector2 lineDir = lineEnd - lineStart;
        float lineLength = lineDir.Length();
        lineDir /= lineLength; // Normalize

        // Line normal (perpendicular)
        Vector2 lineNormal = new Vector2(-lineDir.Y, lineDir.X);

        // Initialize collision info
        normal = Vector2.Zero;
        penetration = float.MaxValue;

        // Project corners onto line normal
        float minCornerProj = float.MaxValue;
        float maxCornerProj = float.MinValue;
        foreach (var corner in corners)
        {
            float proj = Vector2.Dot(corner - lineStart, lineNormal);
            minCornerProj = Math.Min(minCornerProj, proj);
            maxCornerProj = Math.Max(maxCornerProj, proj);
        }

        // Check if rectangle overlaps line along normal
        if (minCornerProj > 0 || maxCornerProj < 0)
            return false;

        // Find closest point on line to rectangle
        Vector2 rectCenter = rectPos + rectSize / 2;
        float centerProj = Vector2.Dot(rectCenter - lineStart, lineDir);
        centerProj = Math.Clamp(centerProj, 0, lineLength);
        Vector2 closestPoint = lineStart + lineDir * centerProj;

        // Check if closest point is within rectangle bounds
        Rectangle rect = new Rectangle(
            (int)rectPos.X, (int)rectPos.Y,
            (int)rectSize.X, (int)rectSize.Y);

        if (!rect.Contains((int)closestPoint.X, (int)closestPoint.Y))
        {
            float expansion = Math.Max(rectSize.X, rectSize.Y) / 2;
            Rectangle expandedRect = new Rectangle(
                (int)(rect.X - expansion),
                (int)(rect.Y - expansion),
                (int)(rect.Width + expansion * 2),
                (int)(rect.Height + expansion * 2));

            if (!expandedRect.Contains((int)closestPoint.X, (int)closestPoint.Y))
                return false;
        }

        // Calculate penetration and normal
        Vector2 toRect = rectCenter - closestPoint;
        float distance = toRect.Length();
        if (distance == 0)
        {
            normal = lineNormal;
            penetration = Math.Min(rectSize.X, rectSize.Y) / 2;
        }
        else
        {
            normal = toRect / distance;
            float rectProj = Math.Abs(normal.X * rectSize.X / 2) + Math.Abs(normal.Y * rectSize.Y / 2);
            penetration = rectProj - distance;
        }

        return penetration > 0;
    }

    private void UpdateCollisionStates(ref CollisionState stateA, ref CollisionState stateB, Vector2 normal)
    {
        float angle = MathF.Atan2(normal.Y, normal.X);
        const float QUARTER_PI = MathF.PI / 4;

        if (angle < -3 * QUARTER_PI || angle >= 3 * QUARTER_PI) // Left
        {
            stateA.Sides |= CollisionFlags.Left;
            stateB.Sides |= CollisionFlags.Right;
        }
        else if (angle < -QUARTER_PI) // Top
        {
            stateA.Sides |= CollisionFlags.Top;
            stateB.Sides |= CollisionFlags.Bottom;
        }
        else if (angle < QUARTER_PI) // Right
        {
            stateA.Sides |= CollisionFlags.Right;
            stateB.Sides |= CollisionFlags.Left;
        }
        else // Bottom
        {
            stateA.Sides |= CollisionFlags.Bottom;
            stateB.Sides |= CollisionFlags.Top;
        }
    }
}