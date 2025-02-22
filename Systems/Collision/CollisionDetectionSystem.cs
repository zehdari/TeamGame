using ECS.Components.Collision;
using ECS.Components.Physics;
using ECS.Components.Animation;

namespace ECS.Systems.Collision;

/// <summary>
/// Handles collision detection between entities using a broadphase-narrowphase approach.
/// Uses Separating Axis Theorem (SAT) for precise polygon collision detection.
/// </summary>
public class CollisionDetectionSystem : SystemBase
{
    /// <summary>
    /// Amount to expand AABBs by during broadphase to account for movement between frames.
    /// </summary>
    private const float BROADPHASE_EXPANSION = 2.0f;

    /// <summary>
    /// Tracks currently active collision contacts between entity pairs.
    /// </summary>
    private HashSet<(Entity, Entity)> ActiveContacts = new();

    /// <summary>
    /// Caches AABB bounds for entities during broadphase collision detection.
    /// </summary>
    private Dictionary<Entity, Rectangle> BroadphaseCache = new();

    /// <summary>
    /// Stores collision contacts detected in the current frame.
    /// </summary>
    private List<Contact> frameContacts = new();

    /// <summary>
    /// Updates the collision detection system, performing broadphase and narrowphase collision checks.
    /// </summary>
    /// <param name="world">The game world containing all entities.</param>
    /// <param name="gameTime">Timing information for the current frame.</param>
    public override void Update(World world, GameTime gameTime)
    {
        frameContacts.Clear();

        var bodies = GetCollidableBodies();
        var pairs = BroadPhase(bodies);
        var contacts = NarrowPhase(pairs);
        ProcessContacts(contacts);
    }

    /// <summary>
    /// Collects all entities that have collision components and can potentially collide.
    /// </summary>
    /// <returns>A list of tuples containing entities and their collision-related components.</returns>
    private List<(Entity, CollisionBody, Position, Velocity?)> GetCollidableBodies()
    {
        var bodies = new List<(Entity, CollisionBody, Position, Velocity?)>();
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<CollisionBody>(entity) || !HasComponents<Position>(entity))
                continue;

            ref var body = ref GetComponent<CollisionBody>(entity);
            ref var pos = ref GetComponent<Position>(entity);
            Velocity? vel = HasComponents<Velocity>(entity) 
                ? GetComponent<Velocity>(entity) 
                : null;

            bodies.Add((entity, body, pos, vel));
        }
        return bodies;
    }

    /// <summary>
    /// Calculates an Axis-Aligned Bounding Box (AABB) that encompasses all polygons of an entity.
    /// </summary>
    /// <param name="entity">The entity to calculate AABB for.</param>
    /// <param name="body">The entity's collision body component.</param>
    /// <param name="pos">The entity's position component.</param>
    /// <returns>A Rectangle representing the AABB.</returns>
    private Rectangle CalculateAABB(Entity entity, CollisionBody body, Position pos)
    {
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;

        foreach (var polygon in body.Polygons)
        {
            var transformed = GetTransformedVertices(entity, polygon, pos);
            foreach (var vertex in transformed)
            {
                minX = Math.Min(minX, vertex.X);
                minY = Math.Min(minY, vertex.Y);
                maxX = Math.Max(maxX, vertex.X);
                maxY = Math.Max(maxY, vertex.Y);
            }
        }

        return new Rectangle(
            (int)minX,
            (int)minY,
            (int)(maxX - minX),
            (int)(maxY - minY)
        );
    }

    /// <summary>
    /// Calculates a swept AABB that encompasses an entity's movement over a frame,
    /// considering all polygons in the collision body.
    /// </summary>
    /// <param name="entity">The entity to calculate swept AABB for</param>
    /// <param name="body">The entity's collision body containing polygons</param>
    /// <param name="pos">The entity's current position</param>
    /// <param name="velocity">The entity's velocity</param>
    /// <param name="deltaTime">Time step to sweep over</param>
    /// <returns>An expanded AABB that covers the movement path of all polygons</returns>
    private Rectangle CalculateSweptAABB(Entity entity, CollisionBody body, Position pos, Vector2 velocity, float deltaTime)
    {
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;
        Vector2 displacement = velocity * deltaTime;

        // For each polygon in the body
        foreach (var polygon in body.Polygons)
        {
            // Get vertices in world space at start position
            var startVertices = GetTransformedVertices(entity, polygon, pos);
            
            // Create a position for the end of the sweep
            var endPos = new Position { Value = pos.Value + displacement };
            var endVertices = GetTransformedVertices(entity, polygon, endPos);

            // Find bounds encompassing both start and end positions
            for (int i = 0; i < startVertices.Length; i++)
            {
                // Check start vertices
                minX = Math.Min(minX, startVertices[i].X);
                minY = Math.Min(minY, startVertices[i].Y);
                maxX = Math.Max(maxX, startVertices[i].X);
                maxY = Math.Max(maxY, startVertices[i].Y);

                // Check end vertices
                minX = Math.Min(minX, endVertices[i].X);
                minY = Math.Min(minY, endVertices[i].Y);
                maxX = Math.Max(maxX, endVertices[i].X);
                maxY = Math.Max(maxY, endVertices[i].Y);
            }
        }

        return new Rectangle(
            (int)minX,
            (int)minY,
            (int)(maxX - minX),
            (int)(maxY - minY)
        );
    }

    private List<(Entity, Entity)> BroadPhase(List<(Entity, CollisionBody, Position, Velocity?)> bodies)
    {
        var pairs = new List<(Entity, Entity)>();
        BroadphaseCache.Clear();

        // Use a consistent time step for sweep calculations
        const float SWEEP_TIME = 1f/60f; // 60FPS is max

        foreach (var (entity, body, pos, vel) in bodies)
        {
            Rectangle sweptAABB;

            if (vel.HasValue && vel.Value.Value != Vector2.Zero)
            {
                // Calculate swept AABB for moving objects
                sweptAABB = CalculateSweptAABB(entity, body, pos, vel.Value.Value, SWEEP_TIME);
                
                // Add small expansion for safety
                sweptAABB.X -= (int)BROADPHASE_EXPANSION;
                sweptAABB.Y -= (int)BROADPHASE_EXPANSION;
                sweptAABB.Width += (int)BROADPHASE_EXPANSION * 2;
                sweptAABB.Height += (int)BROADPHASE_EXPANSION * 2;
            }
            else
            {
                // Use static AABB for non-moving objects
                sweptAABB = CalculateAABB(entity, body, pos);
            }

            BroadphaseCache[entity] = sweptAABB;
        }

        for (int i = 0; i < bodies.Count; i++)
        {
            for (int j = i + 1; j < bodies.Count; j++)
            {
                var (entityA, _, _, velA) = bodies[i];
                var (entityB, _, _, velB) = bodies[j];

                if (!velA.HasValue && !velB.HasValue) continue;

                var aabbA = BroadphaseCache[entityA];
                var aabbB = BroadphaseCache[entityB];

                if (aabbA.Intersects(aabbB))
                {
                    pairs.Add((entityA, entityB));
                }
            }
        }

        return pairs;
    }

    /// <summary>
    /// Performs narrowphase collision detection using the Separating Axis Theorem (SAT).
    /// </summary>
    /// <param name="pairs">List of potentially colliding entity pairs from broadphase.</param>
    /// <returns>List of confirmed collision contacts with detailed collision information.</returns>
    private List<Contact> NarrowPhase(List<(Entity, Entity)> pairs)
    {
        var contacts = new List<Contact>();

        foreach (var (entityA, entityB) in pairs)
        {
            var bodyA = GetComponent<CollisionBody>(entityA);
            var bodyB = GetComponent<CollisionBody>(entityB);
            var posA = GetComponent<Position>(entityA);
            var posB = GetComponent<Position>(entityB);

            foreach (var polygonA in bodyA.Polygons)
            {
                var transformedA = GetTransformedVertices(entityA, polygonA, posA);

                foreach (var polygonB in bodyB.Polygons)
                {
                    if ((polygonA.Layer & polygonB.CollidesWith) == 0 &&
                        (polygonB.Layer & polygonA.CollidesWith) == 0){
                        continue;
                        }

                    var transformedB = GetTransformedVertices(entityB, polygonB, posB);
                    var contact = CheckPolygonCollision(entityA, entityB,
                        transformedA, transformedB,
                        polygonA, polygonB);

                    if (contact.HasValue)
                        contacts.Add(contact.Value);
                }
            }
        }

        return contacts;
    }

    private Vector2[] GetTransformedVertices(Entity entity, Polygon polygon, Position pos) {
        Scale scale = HasComponents<Scale>(entity) ? GetComponent<Scale>(entity) : default;
        return PolygonTools.GetTransformedVertices(entity, polygon, pos, scale);
    }

    /// <summary>
    /// Checks for collision between two polygons using the Separating Axis Theorem.
    /// </summary>
    /// <param name="entityA">First entity in the collision.</param>
    /// <param name="entityB">Second entity in the collision.</param>
    /// <param name="verticesA">Transformed vertices of the first polygon.</param>
    /// <param name="verticesB">Transformed vertices of the second polygon.</param>
    /// <param name="polygonA">First polygon's collision data.</param>
    /// <param name="polygonB">Second polygon's collision data.</param>
    /// <returns>Contact information if collision detected, null otherwise.</returns>
    private Contact? CheckPolygonCollision(
        Entity entityA, Entity entityB,
        Vector2[] verticesA, Vector2[] verticesB,
        Polygon polygonA, Polygon polygonB)
    {
        var axes = GetSATAxes(verticesA, verticesB);
        float minOverlap = float.MaxValue;
        Vector2 minAxis = Vector2.Zero;

        foreach (var axis in axes)
        {
            var projA = PolygonTools.ProjectPolygon(verticesA, axis);
            var projB = PolygonTools.ProjectPolygon(verticesB, axis);
            float overlap = Math.Min(projA.Y - projB.X, projB.Y - projA.X);
            if (overlap < 0)
                return null;

            if (overlap < minOverlap)
            {
                minOverlap = overlap;
                minAxis = axis;
            }
        }

        var centerA = PolygonTools.GetPolygonCenter(verticesA);
        var centerB = PolygonTools.GetPolygonCenter(verticesB);
        var direction = centerB - centerA;
        if (Vector2.Dot(direction, minAxis) < 0)
            minAxis = -minAxis;

        // Zero out near-zero values to counter floating-point precision errors
        const float EPSILON = 0.0001f;
        if (Math.Abs(minAxis.X) < EPSILON) minAxis.X = 0;
        if (Math.Abs(minAxis.Y) < EPSILON) minAxis.Y = 0;

        // Normalize the vector
        minAxis = Vector2.Normalize(minAxis);

        return new Contact
        {
            EntityA = entityA,
            EntityB = entityB,
            Normal = minAxis,
            Point = CalculateContactPoint(verticesA, verticesB, minAxis),
            Penetration = minOverlap,
            TimeOfImpact = 0.0f,
            LayerA = polygonA.Layer,
            LayerB = polygonB.Layer
        };
    }

    /// <summary>
    /// Gets the axes to test for the Separating Axis Theorem from both polygons.
    /// </summary>
    /// <param name="verticesA">Vertices of the first polygon.</param>
    /// <param name="verticesB">Vertices of the second polygon.</param>
    /// <returns>List of normalized axes to test for separation.</returns>
    private List<Vector2> GetSATAxes(Vector2[] verticesA, Vector2[] verticesB)
    {
        var axes = new List<Vector2>();

        // For first polygon
        for (int i = 0; i < verticesA.Length; i++)
        {
            Vector2 edge = verticesA[(i + 1) % verticesA.Length] - verticesA[i];
            axes.Add(Vector2.Normalize(new Vector2(-edge.Y, edge.X)));
        }
        // For second polygon
        for (int i = 0; i < verticesB.Length; i++)
        {
            Vector2 edge = verticesB[(i + 1) % verticesB.Length] - verticesB[i];
            axes.Add(Vector2.Normalize(new Vector2(-edge.Y, edge.X)));
        }

        return axes;
    }

    /// <summary>
    /// Calculates the deepest point of intersection between two polygons.
    /// </summary>
    /// <param name="verticesA">Vertices of the first polygon.</param>
    /// <param name="verticesB">Vertices of the second polygon.</param>
    /// <param name="normal">Normal vector of the collision.</param>
    /// <returns>Point of deepest intersection.</returns>
    private Vector2 CalculateContactPoint(Vector2[] verticesA, Vector2[] verticesB, Vector2 normal)
    {
        int deepestIndex = 0;
        float deepestDot = float.MaxValue;
        for (int i = 0; i < verticesA.Length; i++)
        {
            float dot = Vector2.Dot(verticesA[i], normal);
            if (dot < deepestDot)
            {
                deepestDot = dot;
                deepestIndex = i;
            }
        }
        return verticesA[deepestIndex];
    }

    /// <summary>
    /// Processes collision contacts and publishes appropriate collision events.
    /// Tracks ongoing collisions and generates Begin, Stay, and End events.
    /// </summary>
    /// <param name="contacts">List of contacts detected this frame.</param>
    /// <remarks>
    /// This method maintains the state of active collisions and generates three types of events:
    /// - CollisionEventType.Begin: When two entities start colliding
    /// - CollisionEventType.Stay: When two entities remain in collision
    /// - CollisionEventType.End: When two entities stop colliding
    /// </remarks>
    private void ProcessContacts(List<Contact> contacts)
    {
        var currentContacts = new HashSet<(Entity, Entity)>();

        foreach (var contact in contacts)
        {
            var pair = (contact.EntityA, contact.EntityB);
            currentContacts.Add(pair);

            if (!ActiveContacts.Contains(pair))
            {
                ActiveContacts.Add(pair);
                Publish(new CollisionEvent
                {
                    Contact = contact,
                    EventType = CollisionEventType.Begin
                });
            }
            else
            {
                Publish(new CollisionEvent
                {
                    Contact = contact,
                    EventType = CollisionEventType.Stay
                });
            }
        }

        var endedContacts = ActiveContacts.Except(currentContacts).ToList();
        foreach (var (entityA, entityB) in endedContacts)
        {
            ActiveContacts.Remove((entityA, entityB));
            Publish(new CollisionEvent
            {
                Contact = new Contact { EntityA = entityA, EntityB = entityB },
                EventType = CollisionEventType.End
            });
        }
    }
}