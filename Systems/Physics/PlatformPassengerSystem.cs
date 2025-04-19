using ECS.Components.Collision;
using ECS.Components.Physics;
using ECS.Components.Map;
using ECS.Components.State;
using ECS.Components.Tags;

namespace ECS.Systems.Physics
{
    public class PlatformPassengerSystem : SystemBase
    {
        private const int STICKY_FRAMES = 5;

        // Track platforms' last positions
        private Dictionary<Entity, Vector2> previousPlatformPositions = new Dictionary<Entity, Vector2>();

        // Which platform each entity is riding
        private Dictionary<Entity, Entity> entityToPlatform = new Dictionary<Entity, Entity>();

        // How many frames remain in the sticky window
        private Dictionary<Entity, int> graceFrames = new Dictionary<Entity, int>();

        public override void Initialize(World world)
        {
            base.Initialize(world);
            Subscribe<CollisionEvent>(HandleCollision);
        }

        private void HandleCollision(IEvent evt)
        {
            var collision = (CollisionEvent)evt;

            // Identify platform vs passenger
            Entity? platformEntity = null;
            Entity? passengerEntity = null;
            if (HasComponents<Platform>(collision.Contact.EntityA))
            {
                platformEntity = collision.Contact.EntityA;
                passengerEntity = collision.Contact.EntityB;
            }
            else if (HasComponents<Platform>(collision.Contact.EntityB))
            {
                platformEntity = collision.Contact.EntityB;
                passengerEntity = collision.Contact.EntityA;
            }
            if (platformEntity == null || passengerEntity == null)
                return;

            var platform = platformEntity.Value;
            var passenger = passengerEntity.Value;

            // Only stick players, AI, or items
            if (!HasComponents<PlayerTag>(passenger) &&
                !HasComponents<AITag>(passenger) &&
                !HasComponents<ItemTag>(passenger))
            {
                return;
            }

            // On Begin/Stay: only if landing on top
            if (collision.EventType != CollisionEventType.End)
            {
                bool fromA = collision.Contact.EntityA.Equals(passenger);
                Vector2 normal = fromA
                    ? -collision.Contact.Normal
                    : collision.Contact.Normal;

                if (normal.Y < -0.7f) // Not really trusting grounded system here (should be, but alas)
                {
                    // Start riding (or continue)
                    entityToPlatform[passenger] = platform;
                    graceFrames.Remove(passenger);

                    // Track platform position if first time
                    if (!previousPlatformPositions.ContainsKey(platform) &&
                        HasComponents<Position>(platform))
                    {
                        previousPlatformPositions[platform] =
                            GetComponent<Position>(platform).Value;
                    }
                }
            }
        }

        public override void Update(World world, GameTime gameTime)
        {
            // Handle lost contact with sticky window & direction-change grace
            foreach (var mapping in entityToPlatform.ToList())
            {
                var passenger = mapping.Key;
                var platform = mapping.Value;

                // If the platform itself just changed direction, treat as still touching
                bool platformInDirChange = false;
                if (HasComponents<PlatformDirectionState>(platform))
                {
                    var dirState = GetComponent<PlatformDirectionState>(platform);
                    platformInDirChange = dirState.JustChangedDirection || dirState.DirectionChangeFrames > 0;
                }

                // Check if still physically contacting the platform from the top
                bool stillTouchingTop = false;
                if (HasComponents<ContactState>(passenger))
                {
                    ref var contactState = ref GetComponent<ContactState>(passenger);
                    
                    // Check if we have contact with this platform
                    if (contactState.Contacts != null && contactState.Contacts.TryGetValue(platform, out var contact))
                    {
                        // Determine if the passenger is EntityA or EntityB in the contact
                        bool isPassengerA = passenger.Id == contact.EntityA.Id;
                        
                        // Get normal from passenger's perspective
                        Vector2 normalFromPassenger = isPassengerA ? contact.Normal : -contact.Normal;
                        
                        // Check if this is a top collision (normal pointing up from passenger perspective)
                        stillTouchingTop = normalFromPassenger.Y < -0.7f;
                    }
                }

                if (stillTouchingTop || platformInDirChange)
                {
                    // Reset sticky window while on top or during direction-change grace
                    graceFrames.Remove(passenger);
                }
                else
                {
                    // Not touching: start or tick down sticky
                    if (!graceFrames.ContainsKey(passenger))
                    {
                        graceFrames[passenger] = STICKY_FRAMES;
                    }
                    else if (--graceFrames[passenger] <= 0)
                    {
                        entityToPlatform.Remove(passenger);
                        graceFrames.Remove(passenger);
                    }
                }
            }

            var updatedPlatformPositions = new Dictionary<Entity, Vector2>();

            // For each platform, compute how far it moved
            foreach (var platform in World.GetEntities()
                                          .Where(e => HasComponents<Platform>(e)
                                                   && HasComponents<Position>(e)))
            {
                var currentPos = GetComponent<Position>(platform).Value;

                if (!previousPlatformPositions.TryGetValue(platform, out var prevPos))
                {
                    previousPlatformPositions[platform] = currentPos;
                    updatedPlatformPositions[platform] = currentPos;
                    continue;
                }

                var delta = currentPos - prevPos;
                // Only carry horizontal + downward motion
                float dy = Math.Max(delta.Y, 0f);
                var passengerDelta = new Vector2(delta.X, dy);

                // Move every entity still riding this platform
                foreach (var entry in entityToPlatform.Where(kv => kv.Value.Equals(platform)))
                {
                    var passenger = entry.Key;
                    if (!HasComponents<Position>(passenger))
                        continue;

                    ref var pos = ref GetComponent<Position>(passenger);
                    pos.Value += passengerDelta;
                }

                updatedPlatformPositions[platform] = currentPos;
            }

            // Swap in the new platform positions
            previousPlatformPositions = updatedPlatformPositions;
        }
    }
}