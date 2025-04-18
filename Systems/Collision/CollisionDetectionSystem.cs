using ECS.Components.Collision;
using ECS.Components.Physics;
using ECS.Components.Animation;
using ECS.Components.Map;
using ECS.Core.Utilities;

namespace ECS.Systems.Collision;

public class CollisionDetectionSystem : SystemBase
{
    private HashSet<(Entity, Entity)> ActiveContacts = new();
    private Dictionary<Entity, Rectangle> BroadphaseCache = new();
    private Dictionary<(Entity, Polygon), Vector2[]> transformedVerticesCache = new();
    private Dictionary<int, bool> PlatformDirectionHistory = new();
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
        const int EXPANSION_SCALE = 2;

        var aabb = CalculateAABB(entity, body, pos);

        // Use 10% of the AABB's size as the expansion
        int expansionX = (int)(aabb.Width * 0.1f);
        int expansionY = (int)(aabb.Height * 0.1f);

        // Include a minimum expansion value to avoid a zero or very small expansion
        expansionX = Math.Max(expansionX, EXPANSION_SCALE);
        expansionY = Math.Max(expansionY, EXPANSION_SCALE);

        aabb.X -= expansionX;
        aabb.Y -= expansionY;
        aabb.Width += expansionX * EXPANSION_SCALE;
        aabb.Height += expansionY * EXPANSION_SCALE;

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
            // Handle platform collision logic
            bool skipPlatformCollision = ShouldSkipPlatformCollision(entityA, entityB);
            if (skipPlatformCollision)
                continue;

            var bodyA = GetComponent<CollisionBody>(entityA);
            var bodyB = GetComponent<CollisionBody>(entityB);
            var posA = GetComponent<Position>(entityA);
            var posB = GetComponent<Position>(entityB);

            // Process contacts for this pair of entities
            contacts.AddRange(ProcessEntityPairContacts(entityA, entityB, bodyA, bodyB, posA, posB));
        }

        return contacts;
    }

// Needs split up into smaller methods still, but works for now
private bool ShouldSkipPlatformCollision(Entity entityA, Entity entityB)
{
    bool isPlatformA = HasComponents<Platform>(entityA);
    bool isPlatformB = HasComponents<Platform>(entityB);

    if (!isPlatformA && !isPlatformB)
        return false;  // Not a platform collision

    // Determine which entity is the platform and which is the character
    Entity platform = isPlatformA ? entityA : entityB;
    Entity character = isPlatformA ? entityB : entityA;

    if (!HasComponents<Position>(platform) || !HasComponents<Position>(character))
        return false;

    // Get positions
    var platformPos = GetComponent<Position>(platform).Value;
    var characterPos = GetComponent<Position>(character).Value;
    
    // Calculate precise platform top edge and character feet
    float platformTop = CalculatePlatformTop(platform);
    float characterFeet = 0;
    float characterHeight = 0;
    CalculateCharacterBounds(character, out characterFeet, out characterHeight);
    
    // Get velocities
    Vector2 characterVelocity = HasComponents<Velocity>(character) 
        ? GetComponent<Velocity>(character).Value 
        : Vector2.Zero;

    Vector2 platformVelocity = HasComponents<Velocity>(platform) 
        ? GetComponent<Velocity>(platform).Value 
        : Vector2.Zero;

    Vector2 relativeVelocity = characterVelocity - platformVelocity;
    
    // Initialize traversal state if needed
    if (!HasComponents<PlatformTraversalState>(character))
    {
        World.GetPool<PlatformTraversalState>().Set(character, new PlatformTraversalState
        {
            LastYPosition = characterPos.Y,
            WasGoingUp = characterVelocity.Y < 0,
            JustPassedUp = false,
            IsRequestingDropThrough = false,
            PassedThrough = new HashSet<int>()
        });
    }
    
    ref var traversalState = ref GetComponent<PlatformTraversalState>(character);
    
    // Ensure PassedThrough is initialized
    if (traversalState.PassedThrough == null)
    {
        traversalState.PassedThrough = new HashSet<int>();
    }
    
    int platformId = platform.Id;
    
    // Check for platform direction change
    bool platformChangedDirection = false;
    bool platformInGracePeriod = false;
    
    // Drop-through request takes priority
    if (traversalState.IsRequestingDropThrough)
    {
        traversalState.PassedThrough.Add(platformId);
        return true;
    }

    // Safety penetration threshold
    const float HEIGHT_FRACTION = 0.2f;
    float penetrationThreshold = characterHeight * HEIGHT_FRACTION;

    // If platform recently changed direction, check to force collision
    if (HasComponents<PlatformDirectionState>(platform))
    {
        ref var directionState = ref GetComponent<PlatformDirectionState>(platform);
        platformChangedDirection = directionState.JustChangedDirection;
        platformInGracePeriod = directionState.DirectionChangeFrames > 0;
        
        // Force collision for a few frames when platform changes from up to down
        if ((platformChangedDirection || platformInGracePeriod) && 
            Math.Abs(characterFeet - platformTop) < penetrationThreshold)
        {
            // Clear the passed-through flag and force collision
            traversalState.PassedThrough.Remove(platformId);
            System.Diagnostics.Debug.WriteLine($"Forcing collision due to platform {platformId} direction change");
            System.Diagnostics.Debug.WriteLine($"Character feet: {characterFeet}, Platform top: {platformTop}");
            return false;
        }
    }
    
    // Calculate movement and position states
    bool isGoingUp = relativeVelocity.Y < 0;
    bool justChangedDirection = traversalState.WasGoingUp != isGoingUp;
    
    
    bool isAbovePlatform = characterFeet <= platformTop;
    bool isBelowPlatform = characterFeet > platformTop + penetrationThreshold;
    
    // Rule 1: If moving upward, pass through
    if (isGoingUp)
    {
        if (!traversalState.PassedThrough.Contains(platformId))
        {
            traversalState.PassedThrough.Add(platformId);
        }
        return true;
    }
    
    // Rule 2: If moving downward after going up, clear passed-through status
    if (!isGoingUp && justChangedDirection && !isBelowPlatform)
    {
        traversalState.PassedThrough.Remove(platformId);
    }
    
    // Rule 3: If above platform and moving down, allow landing
    if (isAbovePlatform && !isGoingUp)
    {
        traversalState.PassedThrough.Remove(platformId);
        return false;
    }
    
    // Rule 4: If below platform, skip collision
    if (isBelowPlatform)
    {
        if (!traversalState.PassedThrough.Contains(platformId))
        {
            traversalState.PassedThrough.Add(platformId);
        }
        return true;
    }
    
    // Update state for next frame
    traversalState.LastYPosition = characterPos.Y;
    traversalState.WasGoingUp = isGoingUp;
    
    // Final decision: Skip if marked as passed through
    return traversalState.PassedThrough.Contains(platformId);
}

    // Calculate the top of the platform
    private float CalculatePlatformTop(Entity platform)
    {
        float platformTop = GetComponent<Position>(platform).Value.Y;
        
        if (HasComponents<CollisionBody>(platform))
        {
            ref var body = ref GetComponent<CollisionBody>(platform);
            foreach (var polygon in body.Polygons)
            {
                if (polygon.IsTrigger || polygon.Layer != CollisionLayer.World)
                    continue;
                    
                if (transformedVerticesCache.TryGetValue((platform, polygon), out var vertices))
                {
                    float minY = float.MaxValue;
                    foreach (var vertex in vertices)
                    {
                        minY = Math.Min(minY, vertex.Y);
                    }
                    platformTop = minY;
                    break;
                }
            }
        }
        
        return platformTop;
    }

    // Calculate the feet of the character
    private void CalculateCharacterBounds(Entity character, out float feet, out float height)
    {
        var characterPos = GetComponent<Position>(character).Value;
        feet = characterPos.Y;
        height = 0;
        
        if (HasComponents<CollisionBody>(character))
        {
            ref var body = ref GetComponent<CollisionBody>(character);
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            
            foreach (var polygon in body.Polygons)
            {
                if (polygon.IsTrigger || polygon.Layer != CollisionLayer.Physics)
                    continue;
                    
                if (transformedVerticesCache.TryGetValue((character, polygon), out var vertices))
                {
                    foreach (var vertex in vertices)
                    {
                        minY = Math.Min(minY, vertex.Y);
                        maxY = Math.Max(maxY, vertex.Y);
                    }
                }
            }
            
            feet = maxY;
            height = maxY - minY;
        }
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
        
        // First, clear all contacts in contact states
        foreach (var entity in World.GetEntities())
        {
            if (HasComponents<ContactState>(entity))
            {
                ref var contactState = ref GetComponent<ContactState>(entity);
                // Initialize the Dictionary if null
                if (contactState.Contacts == null)
                    contactState.Contacts = new Dictionary<Entity, Contact>();
                else
                    contactState.Contacts.Clear();
            }
        }

        foreach (var contact in contacts)
        {
            var pair = (contact.EntityA, contact.EntityB);
            currentContacts.Add(pair);

            // Update ContactState for EntityA
            if (!HasComponents<ContactState>(contact.EntityA))
            {
                World.GetPool<ContactState>().Set(contact.EntityA, new ContactState 
                { 
                    Contacts = new Dictionary<Entity, Contact>() 
                });
            }
            ref var contactStateA = ref GetComponent<ContactState>(contact.EntityA);
            if (contactStateA.Contacts == null)
                contactStateA.Contacts = new Dictionary<Entity, Contact>();
            contactStateA.Contacts[contact.EntityB] = contact;

            // Update ContactState for EntityB with the same contact
            if (!HasComponents<ContactState>(contact.EntityB))
            {
                World.GetPool<ContactState>().Set(contact.EntityB, new ContactState 
                { 
                    Contacts = new Dictionary<Entity, Contact>() 
                });
            }
            ref var contactStateB = ref GetComponent<ContactState>(contact.EntityB);
            if (contactStateB.Contacts == null)
                contactStateB.Contacts = new Dictionary<Entity, Contact>();
            contactStateB.Contacts[contact.EntityA] = contact;

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