namespace ECSAttempt.Events;

public struct InputEvent : IEvent
{
    public Vector2 MovementDirection;
    public Entity Entity;
}