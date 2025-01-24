namespace ECSAttempt.Core;

public class ComponentPool<T> where T : struct
{
    private T[] components = new T[1024];
    private Dictionary<int, int> entityToIndex = new();
    private Stack<int> freeIndices = new();
    private int capacity = 1024;

    public int Count => entityToIndex.Count; // Add this property

    public void EnsureCapacity(int newCapacity)
    {
        if (newCapacity <= capacity) return;
        
        var newComponents = new T[newCapacity];
        Array.Copy(components, newComponents, capacity);
        components = newComponents;
        capacity = newCapacity;
    }

    public void Set(Entity entity, in T component)
    {
        if (!entityToIndex.TryGetValue(entity.Id, out int index))
        {
            index = freeIndices.Count > 0 ? freeIndices.Pop() : entityToIndex.Count;
            entityToIndex[entity.Id] = index;
        }
        EnsureCapacity(index + 1);
        components[index] = component;
    }

    public ref T Get(Entity entity) => ref components[entityToIndex[entity.Id]];
    public bool Has(Entity entity) => entityToIndex.ContainsKey(entity.Id);

    public void Remove(Entity entity)
    {
        if (entityToIndex.Remove(entity.Id, out int index))
            freeIndices.Push(index);
    }
}
