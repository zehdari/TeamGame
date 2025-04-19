namespace ECS.Events;

public struct SoundEvent : IEvent
{
    public string SoundKey;
    public bool isMusic;
}