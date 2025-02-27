using ECS.Components.Collision;
using ECS.Components.Physics;
using ECS.Components.Animation;

namespace ECS.Systems.Collision;
public class CollisionDetectionSystem : SystemBase
{
    private const float BROADPHASE_EXPANSION = 2.0f;
    private HashSet<(Entity, Entity)> ActiveContacts = new();
    private Dictionary<Entity, Rectangle> BroadphaseCache = new();
    private float deltaTime;
    private GraphicsManager graphicsManager;

    public CollisionDetectionSystem(GraphicsManager graphicsManager)
    {
        this.graphicsManager = graphicsManager;
    }
    
    public override void Initialize(World world)
    {
        base.Initialize(world);
    }

    public override void Update(World world, GameTime gameTime)
    {
        deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        var bodies = GetCollidableBodies();
        var pairs = BroadPhase(bodies);
        var contacts = NarrowPhase(pairs);
        ProcessContacts(contacts);
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

        // Calculate AABB for all bodies
        foreach (var (entity, body, pos, _) in bodies)
        {
            var aabb = CalculateAABB(entity, body, pos);
            
            // Expand AABB slightly
            aabb.X -= (int)BROADPHASE_EXPANSION;
            aabb.Y -= (int)BROADPHASE_EXPANSION;
            aabb.Width += (int)(BROADPHASE_EXPANSION * 2);
            aabb.Height += (int)(BROADPHASE_EXPANSION * 2);
            
            BroadphaseCache[entity] = aabb;
        }

        // Find all potentially colliding pairs
        for (int i = 0; i < bodies.Count; i++)
        {
            for (int j = i + 1; j < bodies.Count; j++)
            {
                var (entityA, _, _, velA) = bodies[i];
                var (entityB, _, _, velB) = bodies[j];

                // Skip collisions between two static bodies
                bool isStaticA = !velA.HasValue;
                bool isStaticB = !velB.HasValue;
                if (isStaticA && isStaticB) continue;

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

    private List<Contact> NarrowPhase(List<(Entity, Entity)> pairs)
    {
        var contacts = new List<Contact>();

        foreach (var (entityA, entityB) in pairs)
        {
            var bodyA = GetComponent<CollisionBody>(entityA);
            var bodyB = GetComponent<CollisionBody>(entityB);
            var posA = GetComponent<Position>(entityA);
            var posB = GetComponent<Position>(entityB);

            // Group collisions by layer combinations
            var layerCombinations = new Dictionary<(CollisionLayer, CollisionLayer), List<Contact>>();

            // Check all polygon pairs
            foreach (var polygonA in bodyA.Polygons)
            {
                foreach (var polygonB in bodyB.Polygons)
                {
                    // Check if these polygons should collide based on their layers
                    if ((polygonA.Layer & polygonB.CollidesWith) == 0 &&
                        (polygonB.Layer & polygonA.CollidesWith) == 0)
                        continue;

                    var transformedA = GetTransformedVertices(entityA, polygonA, posA);
                    var transformedB = GetTransformedVertices(entityB, polygonB, posB);

                    var contact = CheckPolygonCollision(
                        entityA, entityB,
                        transformedA, transformedB,
                        polygonA, polygonB);

                    if (contact.HasValue)
                    {
                        // Create a key for this layer combination (ordered consistently)
                        var layerKey = polygonA.Layer <= polygonB.Layer 
                            ? (polygonA.Layer, polygonB.Layer) 
                            : (polygonB.Layer, polygonA.Layer);
                            
                        // Add this contact to the appropriate layer group
                        if (!layerCombinations.ContainsKey(layerKey))
                            layerCombinations[layerKey] = new List<Contact>();
                            
                        layerCombinations[layerKey].Add(contact.Value);
                    }
                }
            }

            // For each unique layer combination, find the best contact and add it
            foreach (var layerGroup in layerCombinations)
            {
                // Get the contact with the deepest penetration for this layer combination
                var bestContact = layerGroup.Value.OrderByDescending(c => c.Penetration).First();
                contacts.Add(bestContact);
            }
        }

        return contacts;
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
            TimeOfImpact = 0.0f,  // Always 0 now since we're not using CCD
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