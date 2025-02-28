using ECS.Components.Collision;
using ECS.Components.Physics;
using ECS.Components.Animation;
using ECS.Core.Utilities;

namespace ECS.Systems.Collision;

public class CollisionDetectionSystem : SystemBase
{
    private HashSet<(Entity, Entity)> ActiveContacts = new();
    private Dictionary<Entity, Rectangle> BroadphaseCache = new();
    private Dictionary<(Entity, Polygon), Vector2[]> transformedVerticesCache = new();

    public override void Initialize(World world)
    {
        base.Initialize(world);
    }

    public override void Update(World world, GameTime gameTime)
    {
        // Get all bodies that should be checked for collisions
        var bodies = GetCollidableBodies();

        // Clear and cache transformed vertices for all collidable bodies
        CacheTransformedVertices(bodies);

        // Broad phase: quickly filter out pairs that are definitely not colliding
        var pairs = BroadPhase(bodies);

        // Narrow phase: perform detailed collision detection on potential pairs
        var contacts = NarrowPhase(pairs);

        // Process and publish collision events
        ProcessContacts(contacts);
    }

    // Retrieves all entities with CollisionBody and Position components
    private List<(Entity, CollisionBody, Position, Velocity?)> GetCollidableBodies()
    {
        var bodies = new List<(Entity, CollisionBody, Position, Velocity?)>();
        foreach (var entity in World.GetEntities())
        {
            // Only consider entities that have both CollisionBody and Position
            if (!HasComponents<CollisionBody>(entity) || !HasComponents<Position>(entity))
                continue;

            ref var body = ref GetComponent<CollisionBody>(entity);
            ref var pos = ref GetComponent<Position>(entity);

            // Get the velocity if the entity is dynamic
            Velocity? vel = HasComponents<Velocity>(entity)
                ? GetComponent<Velocity>(entity)
                : null;

            bodies.Add((entity, body, pos, vel));
        }
        return bodies;
    }

    // Clears the cache and precomputes transformed vertices for every polygon in every collidable body
    private void CacheTransformedVertices(List<(Entity, CollisionBody, Position, Velocity?)> bodies)
    {
        transformedVerticesCache.Clear();
        foreach (var (entity, body, pos, _) in bodies)
        {
            Scale scale = HasComponents<Scale>(entity) ? GetComponent<Scale>(entity) : default;
            foreach (var polygon in body.Polygons)
            {
                transformedVerticesCache[(entity, polygon)] = PolygonTools.GetTransformedVertices(entity, polygon, pos, scale);
            }
        }
    }

    // Broad phase: Calculates AABB for each body and finds potentially colliding pairs
    private List<(Entity, Entity)> BroadPhase(List<(Entity, CollisionBody, Position, Velocity?)> bodies)
    {
        var pairs = new List<(Entity, Entity)>();
        BroadphaseCache.Clear();

        // Calculate and cache expanded AABB for all bodies
        foreach (var (entity, body, pos, _) in bodies)
        {
            BroadphaseCache[entity] = GetExpandedAABB(entity, body, pos);
        }

        // Filter dynamic bodies for the outer loop
        var dynamicBodies = bodies.Where(b => b.Item4.HasValue).ToList();

        // For each dynamic body, check against all bodies to avoid static vs static checks
        // O(n^2), but as Knuth says...
        foreach (var (dynamicEntity, _, _, _) in dynamicBodies)
        {
            foreach (var (otherEntity, _, _, otherVelocity) in bodies)
            {
                // Skip self check
                if (dynamicEntity.Id == otherEntity.Id)
                    continue;

                // If both bodies are dynamic, use an ordering check to filter out duplicate checks
                if (otherVelocity.HasValue && dynamicEntity.Id > otherEntity.Id)
                    continue;

                var aabbDynamic = BroadphaseCache[dynamicEntity];
                var aabbOther = BroadphaseCache[otherEntity];
                if (aabbDynamic.Intersects(aabbOther))
                    pairs.Add((dynamicEntity, otherEntity));
            }
        }

        return pairs;
    }

    // Returns an expanded AABB for an entity
    private Rectangle GetExpandedAABB(Entity entity, CollisionBody body, Position pos)
    {
        var aabb = CalculateAABB(entity, body, pos);

        // Use 10% of the AABB's size as the expansion
        int expansionX = (int)(aabb.Width * 0.1f);
        int expansionY = (int)(aabb.Height * 0.1f);

        // Include a minimum expansion value to avoid a zero or very small expansion
        expansionX = Math.Max(expansionX, 2);
        expansionY = Math.Max(expansionY, 2);

        aabb.X -= expansionX;
        aabb.Y -= expansionY;
        aabb.Width += expansionX * 2;
        aabb.Height += expansionY * 2;

        return aabb;
    }

    // Calculates the Axis-Aligned Bounding Box (AABB) for an entity using cached transformed vertices
    private Rectangle CalculateAABB(Entity entity, CollisionBody body, Position pos)
    {
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;

        foreach (var polygon in body.Polygons)
        {
            // Retrieve the cached transformed vertices for this polygon
            var transformed = transformedVerticesCache[(entity, polygon)];
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

    // Narrow phase: Performs detailed collision checks between potentially colliding pairs
    private List<Contact> NarrowPhase(List<(Entity, Entity)> pairs)
    {
        var contacts = new List<Contact>();

        foreach (var (entityA, entityB) in pairs)
        {
            var bodyA = GetComponent<CollisionBody>(entityA);
            var bodyB = GetComponent<CollisionBody>(entityB);
            var posA = GetComponent<Position>(entityA);
            var posB = GetComponent<Position>(entityB);

            // Process contacts for this pair of entities
            contacts.AddRange(ProcessEntityPairContacts(entityA, entityB, bodyA, bodyB, posA, posB));
        }

        return contacts;
    }

    // Processes collision detection for a pair of entities by checking all polygon pairs using cached vertices
    private List<Contact> ProcessEntityPairContacts(Entity entityA, Entity entityB, CollisionBody bodyA, CollisionBody bodyB, Position posA, Position posB)
    {
        var contacts = new List<Contact>();
        var layerCombinations = new Dictionary<(CollisionLayer, CollisionLayer), List<Contact>>();

        // Loop through each polygon in the first and second bodies
        foreach (var polygonA in bodyA.Polygons)
        {
            foreach (var polygonB in bodyB.Polygons)
            {
                // Only check if the layers indicate that these polygons can collide
                if ((polygonA.Layer & polygonB.CollidesWith) == 0 &&
                    (polygonB.Layer & polygonA.CollidesWith) == 0)
                    continue;

                // Retrieve cached transformed vertices for both polygons
                var transformedA = transformedVerticesCache[(entityA, polygonA)];
                var transformedB = transformedVerticesCache[(entityB, polygonB)];

                // Check for collision between the two polygons using SAT
                var contact = CheckPolygonCollision(entityA, entityB, transformedA, transformedB, polygonA, polygonB);
                if (contact.HasValue)
                {
                    // Create a consistent key for the collision layers
                    var layerKey = polygonA.Layer <= polygonB.Layer
                        ? (polygonA.Layer, polygonB.Layer)
                        : (polygonB.Layer, polygonA.Layer);

                    // Add the contact to its layer group
                    if (!layerCombinations.ContainsKey(layerKey))
                        layerCombinations[layerKey] = new List<Contact>();
                    layerCombinations[layerKey].Add(contact.Value);
                }
            }
        }

        // For each group, select the contact with the highest penetration
        foreach (var layerGroup in layerCombinations)
        {
            var bestContact = layerGroup.Value.OrderByDescending(c => c.Penetration).First();
            contacts.Add(bestContact);
        }

        return contacts;
    }

    // Checks collision between two polygons using the Separating Axis Theorem
    private Contact? CheckPolygonCollision(
        Entity entityA, Entity entityB,
        Vector2[] verticesA, Vector2[] verticesB,
        Polygon polygonA, Polygon polygonB)
    {
        var axes = GetSATAxes(verticesA, verticesB);
        float minOverlap = float.MaxValue;
        Vector2 minAxis = Vector2.Zero;

        // Check all potential separating axes
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

        // Determine the collision normal direction
        var centerA = PolygonTools.GetPolygonCenter(verticesA);
        var centerB = PolygonTools.GetPolygonCenter(verticesB);
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
            TimeOfImpact = 0.0f,  // Always 0 for now since we're not using continuous collision detection
            LayerA = polygonA.Layer,
            LayerB = polygonB.Layer
        };
    }

    // Returns the list of potential separating axes from both polygons
    private List<Vector2> GetSATAxes(Vector2[] verticesA, Vector2[] verticesB)
    {
        var axes = new List<Vector2>();

        // Get axes from the first polygon's edges
        for (int i = 0; i < verticesA.Length; i++)
        {
            Vector2 edge = verticesA[(i + 1) % verticesA.Length] - verticesA[i];
            axes.Add(Vector2.Normalize(new Vector2(-edge.Y, edge.X)));
        }

        // Get axes from the second polygon's edges
        for (int i = 0; i < verticesB.Length; i++)
        {
            Vector2 edge = verticesB[(i + 1) % verticesB.Length] - verticesB[i];
            axes.Add(Vector2.Normalize(new Vector2(-edge.Y, edge.X)));
        }

        return axes;
    }

    // Calculates the contact point between two polygons
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

    // Processes contacts and publishes collision events (Begin, Stay, End)
    private void ProcessContacts(List<Contact> contacts)
    {
        var currentContacts = new HashSet<(Entity, Entity)>();

        foreach (var contact in contacts)
        {
            var pair = (contact.EntityA, contact.EntityB);
            currentContacts.Add(pair);

            if (!ActiveContacts.Contains(pair))
            {
                // New collision detected
                ActiveContacts.Add(pair);
                Publish(new CollisionEvent
                {
                    Contact = contact,
                    EventType = CollisionEventType.Begin
                });
            }
            else
            {
                // Ongoing collision
                Publish(new CollisionEvent
                {
                    Contact = contact,
                    EventType = CollisionEventType.Stay
                });
            }
        }

        // Handle collisions that have ended
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