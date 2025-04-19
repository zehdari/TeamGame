namespace ECS.Components.Timer;

public struct Timers
{
    public Dictionary<TimerType, Timer> TimerMap;
}

public enum TimerType {
    None = 0,
    AITimer,
    StateTimer,
    ProjectileTimer,
    HitboxTimer,
    SpecialTimer,
    JabTimer,
    EffectTimer,
    ZombieSpawningTimer
}

public struct Timer
{
    public float Duration; // How long the timer is set for
    public float Elapsed; // How much time has passed already
    public TimerType Type;
    public bool OneShot;

}