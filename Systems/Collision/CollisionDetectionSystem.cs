using ECS.Components.Collision;
using ECS.Components.Physics;
using ECS.Components.Animation;
using ECS.Core.Utilities;

namespace ECS.Systems.Collision;
public class CollisionDetectionSystem : SystemBase
{
    private const float BROADPHASE_EXPANSION = 2.0f;
    private HashSet<(Entity, Entity)> ActiveContacts = new();
    private Dictionary<Entity, Rectangle> BroadphaseCache = new();
    private List<Contact> frameContacts = new();
    private float deltaTime;
    private GraphicsManager graphicsManager;
    public SpatialGrid spatialGrid { get; private set; }

    public CollisionDetectionSystem(GraphicsManager graphicsManager)
    {
        this.graphicsManager = graphicsManager;
    }
    public override void Initialize(World world)
    {
        base.Initialize(world);
        CreateSpatialGrid();

    }

    private void CreateSpatialGrid()
    {
        spatialGrid = graphicsManager.spatialGrid;
    }

    public override void Update(World world, GameTime gameTime)
    {
        deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        frameContacts.Clear();

        var bodies = GetCollidableBodies();
        var pairs = BroadPhase(bodies);
        var contacts = NarrowPhase(pairs);
        ProcessContacts(contacts);
    }

    private List<(Entity, Entity)> GetCollisionPairs(List<(Entity, CollisionBody, Position, Velocity?)> bodies)
    {
        var pairs = new List<(Entity, Entity)>();
        
        foreach (var (entity, _, _, vel) in bodies)
        {
            var bounds = BroadphaseCache[entity];
            var potentialCollisions = spatialGrid.GetPotentialCollisions(entity, bounds);

            foreach (var other in potentialCollisions)
            {
                if (entity.Id >= other.Id) continue; // Avoid duplicates and self-collisions

                var otherBody = bodies.Find(b => b.Item1.Equals(other));
                if (otherBody.Item4.HasValue || vel.HasValue) // At least one must be dynamic
                {
                    if ((!vel.HasValue || vel.Value.Value == Vector2.Zero) && 
                        (!otherBody.Item4.HasValue || otherBody.Item4.Value.Value == Vector2.Zero))
                        continue; // Skip if both have zero velocity

                    pairs.Add((entity, other));
                }
            }
        }

        return pairs;
    }

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

    private List<(Entity, Entity)> BroadPhase(List<(Entity, CollisionBody, Position, Velocity?)> bodies)
    {
        var pairs = new List<(Entity, Entity)>();
        BroadphaseCache.Clear();

        foreach (var (entity, body, pos, vel) in bodies)
        {
            Rectangle sweptAABB;

            if (vel.HasValue && vel.Value.Value != Vector2.Zero)
            {
                // Calculate swept AABB for moving objects
                sweptAABB = CalculateSweptAABB(entity, body, pos, vel.Value.Value, deltaTime);
                
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

                // Skip static vs static collisions - require at least one dynamic body
                if (!velA.HasValue && !velB.HasValue) continue;
                
                // Skip collisions between two bodies with zero velocity
                if ((!velA.HasValue || velA.Value.Value == Vector2.Zero) && 
                    (!velB.HasValue || velB.Value.Value == Vector2.Zero)) continue;

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

    private Rectangle CalculateSweptAABB(Entity entity, CollisionBody body, Position pos, Vector2 velocity, float deltaTime)
    {
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;
        Vector2 displacement = velocity * deltaTime;

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
                minX = Math.Min(minX, Math.Min(startVertices[i].X, endVertices[i].X));
                minY = Math.Min(minY, Math.Min(startVertices[i].Y, endVertices[i].Y));
                maxX = Math.Max(maxX, Math.Max(startVertices[i].X, endVertices[i].X));
                maxY = Math.Max(maxY, Math.Max(startVertices[i].Y, endVertices[i].Y));
            }
        }

        return new Rectangle(
            (int)minX,
            (int)minY,
            (int)(maxX - minX),
            (int)(maxY - minY)
        );
    }

    private List<Contact> NarrowPhase(List<(Entity, Entity)> pairs)
    {
        var contacts = new List<Contact>();

        foreach (var (entityA, entityB) in pairs)
        {
            var bodyA = GetComponent<CollisionBody>(entityA);
            var bodyB = GetComponent<CollisionBody>(entityB);
            var posA = GetComponent<Position>(entityA);
            var posB = GetComponent<Position>(entityB);
            var velA = HasComponents<Velocity>(entityA) ? GetComponent<Velocity>(entityA).Value : Vector2.Zero;
            var velB = HasComponents<Velocity>(entityB) ? GetComponent<Velocity>(entityB).Value : Vector2.Zero;

            foreach (var polygonA in bodyA.Polygons)
            {
                foreach (var polygonB in bodyB.Polygons)
                {
                    if ((polygonA.Layer & polygonB.CollidesWith) == 0 &&
                        (polygonB.Layer & polygonA.CollidesWith) == 0)
                        continue;

                    var contact = ContinuousCollisionCheck(
                        entityA, entityB,
                        polygonA, polygonB,
                        posA, posB,
                        velA, velB,
                        deltaTime);

                    if (contact.HasValue)
                    {
                        contacts.Add(contact.Value);
                    }
                }
            }
        }

        return contacts;
    }

    private Contact? ContinuousCollisionCheck(
        Entity entityA, Entity entityB,
        Polygon polygonA, Polygon polygonB,
        Position posA, Position posB,
        Vector2 velA, Vector2 velB,
        float deltaTime)
    {
        // Calculate relative velocity
        Vector2 relativeVel = velB - velA;
        if (relativeVel == Vector2.Zero)
        {
            // If no relative motion, do regular SAT check
            return CheckPolygonCollision(entityA, entityB,
                GetTransformedVertices(entityA, polygonA, posA),
                GetTransformedVertices(entityB, polygonB, posB),
                polygonA, polygonB);
        }

        // Initial positions
        var transformedA = GetTransformedVertices(entityA, polygonA, posA);
        var transformedB = GetTransformedVertices(entityB, polygonB, posB);

        // Do binary search to find Time of Impact (TOI)
        float tMin = 0;
        float tMax = deltaTime;
        const float TOLERANCE = 0.001f;
        const int MAX_ITERATIONS = 20;
        float toi = deltaTime;
        bool foundCollision = false;

        for (int iter = 0; iter < MAX_ITERATIONS; iter++)
        {
            float midTime = (tMin + tMax) / 2;
            
            // Interpolate positions at midTime
            var interpPosA = new Position { Value = posA.Value + velA * midTime };
            var interpPosB = new Position { Value = posB.Value + velB * midTime };
            
            var interpVertsA = GetTransformedVertices(entityA, polygonA, interpPosA);
            var interpVertsB = GetTransformedVertices(entityB, polygonB, interpPosB);

            if (PolygonsOverlap(interpVertsA, interpVertsB))
            {
                tMax = midTime;
                toi = midTime;
                foundCollision = true;
            }
            else
            {
                tMin = midTime;
            }

            if (tMax - tMin < TOLERANCE)
                break;
        }

        if (!foundCollision)
            return null;

        // Calculate contact at TOI
        var contactPosA = new Position { Value = posA.Value + velA * toi };
        var contactPosB = new Position { Value = posB.Value + velB * toi };
        
        var vertsAtContactA = GetTransformedVertices(entityA, polygonA, contactPosA);
        var vertsAtContactB = GetTransformedVertices(entityB, polygonB, contactPosB);

        var contact = CheckPolygonCollision(entityA, entityB, vertsAtContactA, vertsAtContactB, polygonA, polygonB);
        if (contact.HasValue)
        {
            var finalContact = contact.Value;
            finalContact.TimeOfImpact = toi / deltaTime; // Normalize to 0-1
            return finalContact;
        }

        return null;
    }

    private bool PolygonsOverlap(Vector2[] vertsA, Vector2[] vertsB)
    {
        foreach (var axis in GetSATAxes(vertsA, vertsB))
        {
            var projA = ProjectPolygon(vertsA, axis);
            var projB = ProjectPolygon(vertsB, axis);
            if (projA.X > projB.Y || projB.X > projA.Y)
                return false;
        }
        return true;
    }

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
            var projA = ProjectPolygon(verticesA, axis);
            var projB = ProjectPolygon(verticesB, axis);
            float overlap = Math.Min(projA.Y - projB.X, projB.Y - projA.X);
            if (overlap < 0)
                return null;

            if (overlap < minOverlap)
            {
                minOverlap = overlap;
                minAxis = axis;
            }
        }

        var centerA = GetPolygonCenter(verticesA);
        var centerB = GetPolygonCenter(verticesB);
        var direction = centerB - centerA;
        if (Vector2.Dot(direction, minAxis) < 0)
            minAxis = -minAxis;

        const float EPSILON = 0.0001f;
        if (Math.Abs(minAxis.X) < EPSILON) minAxis.X = 0;
        if (Math.Abs(minAxis.Y) < EPSILON) minAxis.Y = 0;
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

    private Vector2[] GetTransformedVertices(Entity entity, Polygon polygon, Position pos)
    {
        Matrix transformMatrix;
        if (HasComponents<Scale>(entity))
        {
            ref var scale = ref GetComponent<Scale>(entity);
            transformMatrix =
                Matrix.CreateScale(scale.Value.X, scale.Value.Y, 1f) *
                Matrix.CreateTranslation(pos.Value.X, pos.Value.Y, 0f);
        }
        else
        {
            transformMatrix = Matrix.CreateTranslation(pos.Value.X, pos.Value.Y, 0f);
        }

        var transformed = new Vector2[polygon.Vertices.Length];
        for (int i = 0; i < polygon.Vertices.Length; i++)
        {
            transformed[i] = Vector2.Transform(polygon.Vertices[i], transformMatrix);
        }
        return transformed;
    }

    private List<Vector2> GetSATAxes(Vector2[] verticesA, Vector2[] verticesB)
    {
        var axes = new List<Vector2>();

        // Get axes from first polygon
        for (int i = 0; i < verticesA.Length; i++)
        {
            Vector2 edge = verticesA[(i + 1) % verticesA.Length] - verticesA[i];
            axes.Add(Vector2.Normalize(new Vector2(-edge.Y, edge.X)));
        }

        // Get axes from second polygon
        for (int i = 0; i < verticesB.Length; i++)
        {
            Vector2 edge = verticesB[(i + 1) % verticesB.Length] - verticesB[i];
            axes.Add(Vector2.Normalize(new Vector2(-edge.Y, edge.X)));
        }

        return axes;
    }

    private Vector2 GetPolygonCenter(Vector2[] vertices)
    {
        Vector2 center = Vector2.Zero;
        foreach (var vertex in vertices)
        {
            center += vertex;
        }
        return center / vertices.Length;
    }

    private (float X, float Y) ProjectPolygon(Vector2[] vertices, Vector2 axis)
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

    private Vector2 CalculateContactPoint(Vector2[] verticesA, Vector2[] verticesB, Vector2 normal)
    {
        // Find the deepest penetrating vertex from polygon A
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