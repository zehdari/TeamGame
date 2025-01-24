namespace ECSAttempt.Core;

public class EventBus
{
    private Dictionary<Type, List<Action<IEvent>>> subscribers = new();

    public void Publish<T>(T eventData) where T : IEvent
    {
        var eventType = typeof(T);
        if (subscribers.ContainsKey(eventType))
        {
            foreach (var subscriber in subscribers[eventType])
            {
                subscriber(eventData);
            }
        }
    }

    public void Subscribe<T>(Action<IEvent> handler) where T : IEvent
    {
        var eventType = typeof(T);
        if (!subscribers.ContainsKey(eventType))
        {
            subscribers[eventType] = new List<Action<IEvent>>();
        }
        subscribers[eventType].Add(handler);
    }
}