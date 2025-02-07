namespace ECS.Core.Utilities;

public static class ComponentUtils
{
    public static void ApplyComponents(World world, Entity entity, EntityConfig config)
    {
        foreach (var componentEntry in config.Components)
        {
            var componentType = componentEntry.Key;
            var componentValue = componentEntry.Value;

            var poolMethod = world.GetType()
                .GetMethod("GetPool")
                ?.MakeGenericMethod(componentType)
                .Invoke(world, null);

            var setMethod = poolMethod?.GetType().GetMethod("Set");
            setMethod?.Invoke(poolMethod, new[] { entity, componentValue });
        }
    }
}