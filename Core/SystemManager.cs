namespace ECS.Core;

public class SystemManager
{
    private readonly Dictionary<SystemExecutionPhase, List<SystemInfo>> systemsByPhase = new();
    private readonly World world;
    private bool needsSort = false;

    public SystemManager(World world)
    {
        this.world = world;
        foreach (SystemExecutionPhase phase in Enum.GetValues(typeof(SystemExecutionPhase)))
        {
            systemsByPhase[phase] = new List<SystemInfo>();
        }
    }

    public void AddSystem(ISystem system, SystemExecutionPhase phase, int priority = 0)
    {
        var systemInfo = new SystemInfo(system, phase, priority);
        systemsByPhase[phase].Add(systemInfo);
        system.Initialize(world);
        needsSort = true;
    }

    public void RemoveSystem(ISystem system)
    {
        foreach (var systems in systemsByPhase.Values)
        {
            systems.RemoveAll(info => info.System == system);
        }
    }

    public void UpdatePhase(SystemExecutionPhase phase, GameTime gameTime)
    {
        if (needsSort)
        {
            SortSystems();
        }

        foreach (var systemInfo in systemsByPhase[phase])
        {
            systemInfo.System.Update(world, gameTime);
        }
    }

    private void SortSystems()
    {
        foreach (var systems in systemsByPhase.Values)
        {
            systems.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }
        needsSort = false;
    }

    public IEnumerable<ISystem> GetAllSystems()
    {
        return systemsByPhase.Values
            .SelectMany(systems => systems)
            .Select(info => info.System);
    }
}