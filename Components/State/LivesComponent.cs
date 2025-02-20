namespace ECS.Components.State;

public class LivesComponent
{
    public int Lives { get; private set; } = 3; // Default to 3 lives

    public LivesComponent(int initialLives)
	{
        Lives = initialLives;
	}
}
