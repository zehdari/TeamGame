namespace ECS.Core;

public interface ISystem
{
    void Initialize(World world);
    void Update(World world, GameTime gameTime);
}