# C# Syntax Cheat Sheet

## Generic Constraints

`where T : struct` Restricts generic type to value types

```csharp
public class ComponentPool<T> where T : struct
public static T ParseJson<T>(string jsonContent) where T : struct
```

## Null Operators

`??=` (Null-coalescing Assignment Operator) Assigns right-hand value only if left-hand is null

`?.` (Null-conditional Operator) Safely accesses members, returns null if object is null

```csharp
options ??= DefaultOptions;
removeMethod?.Invoke(pool, new object[] { entity });
```

## Pattern Matching

`is` Type checking and casting in one operation

`ref` Creates reference to value type

```csharp
if (evt is GameExitEvent gameExitEvent)
ref var state = ref GetComponent<AnimationState>(entity);
```

## Lambda Expressions

`=>` Creates anonymous functions with different parameter patterns:

```csharp
systems.Sort((a, b) => b.Priority.CompareTo(a.Priority));
```

## Dictionary Operations

`TryGetValue` Safe dictionary access returning success status

```csharp
if (!componentPools.TryGetValue(type, out var pool))
{
    pool = new ComponentPool<T>();
    componentPools[type] = pool;
}
```

## Collection Operations

`(x, y)` Tuple destructuring in foreach

`out` Returns additional value through parameter

```csharp
foreach (var (actionName, action) in config.Actions)
entityToIndex.Remove(entity.Id, out int index)
```

## Delegates and Events

Delegates are type-safe function pointers that can reference methods with a specific signature. Good for event driven systems.

`Action<T>` Delegate type for methods taking parameters with no return

```csharp
private Dictionary<Type, List<Action<IEvent>>> subscribers;
public void Subscribe<T>(Action<IEvent> handler) where T : IEvent;
```

## Global Using Directives

`global using` Project-wide namespace import

```csharp
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
```

## Method Expression Bodies

`=>` Shorthand for single-expression methods/properties

```csharp
public Entity(int id) => Id = id;
public override int GetHashCode() => Id;
public override bool Equals(object obj) => 
    obj is Entity other && other.Id == Id;
```

## Object Initialization

`new()` Infers type from context  

```csharp
private readonly Dictionary<Type, object> componentPools = new();
private readonly HashSet<Entity> entities = new();
private Stack<int> freeIndices = new();
```

## Method Access

`GetType()` Gets type information  

`GetMethod()` Gets method info by name

```csharp
var removeMethod = pool.GetType().GetMethod("Remove");
removeMethod?.Invoke(pool, new object[] { entity });
```

---

[**Previous Page**](README.md)
