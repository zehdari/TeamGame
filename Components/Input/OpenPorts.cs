namespace ECS.Components.Input;

public struct OpenPorts
{ 
    public PlayerPorts port;
}

public enum PlayerPorts
{
    PlayerOne,
    PlayerTwo,
    PlayerThree,
    PlayerFour,
    AcceptsAll
}