using ECS.Components.Tags;
using ECS.Components.State;
using System.Diagnostics;

namespace ECS.Core;

public class SystemManager
{
    private readonly Dictionary<SystemExecutionPhase, List<SystemInfo>> systemsByPhase = new();
    private readonly World world;

    // Profiling members
    private readonly Dictionary<string, (double TotalTime, int Count)> executionTimeHistory = new();
    private bool needsSort = false;
    private int loopCount = 0;
    private const int LogInterval = 1000;
    public bool ProfilingEnabled { get; set; } = true;

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

        var gameStateEntity = world.GetEntities()
            .First(e => world.GetPool<GameStateComponent>().Has(e) && 
                        world.GetPool<SingletonTag>().Has(e));

        ref var gameState = ref world.GetPool<GameStateComponent>().Get(gameStateEntity);
        bool isPaused = gameState.CurrentState == GameState.Paused;

        foreach (var systemInfo in systemsByPhase[phase])
        {       
            if (isPaused && systemInfo.System.Pausible)
            {
                continue;
            }

            if (ProfilingEnabled)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                systemInfo.System.Update(world, gameTime);
                stopwatch.Stop();

                double elapsedMicroseconds = (stopwatch.ElapsedTicks / (double)Stopwatch.Frequency) * 1_000_000;
                string systemName = systemInfo.System.GetType().Name;

                if (!executionTimeHistory.ContainsKey(systemName))
                {
                    executionTimeHistory[systemName] = (0, 0);
                }

                var (totalTime, count) = executionTimeHistory[systemName];
                executionTimeHistory[systemName] = (totalTime + elapsedMicroseconds, count + 1);
            }
            else
            {
                systemInfo.System.Update(world, gameTime);
            }
        }
        
        if (ProfilingEnabled) LogExecutionTimes();
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

    private void LogExecutionTimes()
    {
        loopCount++;
        if (loopCount < LogInterval) return;

        Console.WriteLine("\nSystem Execution Times (Average over last {0} loops, µs):\n", LogInterval);

        foreach (var systems in systemsByPhase.Values) 
        {
            foreach (var systemInfo in systems)
            {
                string systemName = systemInfo.System.GetType().Name;

                if (executionTimeHistory.TryGetValue(systemName, out var timeData) && timeData.Count > 0)
                {
                    double averageTime = timeData.TotalTime / timeData.Count;
                    Console.WriteLine($"{systemName}: {averageTime:F3} µs");
                }
            }
        }

        executionTimeHistory.Clear();
        loopCount = 0;
    }


}
