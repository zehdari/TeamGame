namespace ECS.Core;

/// <summary>
/// The event bus, a central system for managing event publication and subscription.
/// Any subscriber to the type will recieve the published event.
/// Each <see cref="Action"/> handles an event of type <see cref="IEvent"/>.
/// </summary>
public class EventBus
{
    /// <summary>
    /// Dictionary to store all subscriber delegates for each type of <see cref="IEvent"/>.
    /// </summary>
    private Dictionary<Type, List<Action<IEvent>>> subscribers = new();

    /// <summary>
    /// Publishes an <see cref="IEvent"/> to all subscribers of that event type <typeparamref name="T"/>.
    /// Iterates over the list of subscribers for the event type and invokes each handler with the event data.
    /// </summary>
    /// <typeparam name="T">The type of the event being published.</typeparam>
    /// <param name="eventData">The data associated with the event being published.</param>
    public void Publish<T>(T eventData) where T : IEvent
    {
        var eventType = typeof(T);

        // Check if there are any subscribers for the event type.
        if (subscribers.ContainsKey(eventType))
        {
            // For each subscriber, invoke the handler with the provided event data.
            foreach (var subscriber in subscribers[eventType])
            {
                subscriber(eventData);
            }
        }
    }

    /// <summary>
    /// Subscribes a handler to a specific event type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the event being subscribed to.</typeparam>
    /// <param name="handler">The delegate to be executed when the event is published.</param>
    public void Subscribe<T>(Action<IEvent> handler) where T : IEvent
    {
        var eventType = typeof(T);

        // If no existing subscribers for this event type, initialize a new list of subscribers.
        if (!subscribers.ContainsKey(eventType))
        {
            subscribers[eventType] = new List<Action<IEvent>>();
        }

        // Add the handler to the list of subscribers for the event type.
        subscribers[eventType].Add(handler);
    }
}