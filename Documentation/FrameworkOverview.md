# ECS Framework Overview

## Architecture Overview

### Core Concepts

1. **Entities**: Simple ID containers that serve as unique identifiers for game objects
2. **Components**: Pure data structures that define entity properties (e.g., Position, Velocity)
3. **Systems**: Logic processors that operate on entities with specific component combinations
4. **World**: The main container that manages entities, components, and systems
5. **Event Bus**: Handles communication between systems using events

### System Execution Phases

Systems are executed in a specific order based on phases with a priority field as a secondary sort:

1. `Input`: Handle raw input and generate events
2. `PreUpdate`: Pre-processing before main update
3. `Update`: Main game logic
4. `PostUpdate`: Post-processing after main update
5. `Render`: Handle all rendering operations

## Component System

### Creating Components

Components are simple data structures. Create new components as structs in the `ECS.Components` namespace:

```csharp
namespace ECS.Components;

public struct MyNewComponent
{
    public float Value;
    public Vector2 Direction;
}
```

### Working with Components

```csharp
// Add component to entity
world.GetPool<MyNewComponent>().Set(entity, new MyNewComponent 
{
    Value = 1.0f,
    Direction = Vector2.One
});

// Check if entity has component
if (HasComponents<MyNewComponent>(entity))
{
    // Get component reference
    ref var component = ref GetComponent<MyNewComponent>(entity);
    component.Value = 2.0f;
}
```

## Event System

### Event Types

Events are structs that implement `IEvent`. Each event should include the `Entity` that triggered it:

```csharp
public struct MyNewEvent : IEvent
{
    public Entity Entity;
    public float Value;
}
```

### Using Events

```csharp
// Subscribe to events
World.EventBus.Subscribe<MyNewEvent>(HandleMyNewEvent);

// Publish events
World.EventBus.Publish(new MyNewEvent 
{ 
    Entity = entity,
    Value = 1.0f 
});

// Handle events
private void HandleMyNewEvent(IEvent evt)
{
    var myEvent = (MyNewEvent)evt;
    // Handle the event
}
```

## Creating New Systems

1. Create a new class in the `ECS.Systems` namespace
2. Inherit from `SystemBase`
3. Implement required methods:

```csharp
namespace ECS.Systems;

public class MyNewSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        // Subscribe to events if needed
        World.EventBus.Subscribe<MyNewEvent>(HandleMyNewEvent);
    }

    public override void Update(World world, GameTime gameTime)
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<RequiredComponent1>(entity) || 
                !HasComponents<RequiredComponent2>(entity))
                continue;

            ref var comp1 = ref GetComponent<RequiredComponent1>(entity);
            ref var comp2 = ref GetComponent<RequiredComponent2>(entity);

            // Process components
        }
    }

    private void HandleMyNewEvent(IEvent evt)
    {
        var myEvent = (MyNewEvent)evt;
        // Handle event
    }
}
```

4. Register the system in `Game1.Initialize()`:

```csharp
world.AddSystem(new MyNewSystem(), SystemExecutionPhase.Update, priority);
```

## Entity Factory

The `EntityFactory` class provides a centralized place to create complex entities. Add new entity creation methods here:

```csharp
public Entity CreateMyNewEntity(params...)
{
    var entity = world.CreateEntity();
    
    // Add required components
    world.GetPool<Component1>().Set(entity, new Component1());
    world.GetPool<Component2>().Set(entity, new Component2());
    
    return entity;
}
```

## Resource Loading

Use the provided loaders for JSON configuration:

- `JsonLoader`: Generic JSON loading
- `InputConfigLoader`: Input configuration
- `SpriteSheetLoader`: Sprite sheet and animation data

Example:

```csharp
var config = InputConfigLoader.LoadInputConfig(File.ReadAllText("Config/input.json"));
```
