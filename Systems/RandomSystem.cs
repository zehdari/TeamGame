
using ECS.Events;

namespace ECS.Systems
{
    public class RandomSystem : SystemBase
    {
        public override void Update(World world, GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Should I make a Random Entity to store this in? TODO
            Random rng = new Random();

            foreach (Entity entity in World.GetEntities())
            {
                if (!HasComponents<RandomRange>(entity))
                    continue;

                ref var randomRange = ref GetComponent<RandomRange>(entity);

                if(HasComponents<RandomlyGeneratedInteger>(entity))
                {
                    ref var randomlyGeneratedInteger = ref GetComponent<RandomlyGeneratedInteger>(entity);
                    randomlyGeneratedInteger.Value = rng.Next(randomRange.Minimum, randomRange.Maximum);
                }
                if (HasComponents<RandomlyGeneratedFloat>(entity))
                {
                    // Doing math and generation as doubles for better accuracy
                    ref var randomlyGeneratedFloat = ref GetComponent<RandomlyGeneratedFloat>(entity);
                    double range = (double)randomRange.Maximum - (double)randomRange.Minimum;
                    randomlyGeneratedFloat.Value = (float)(rng.NextDouble() * range) + randomRange.Minimum;
                }
            }
        }
    }
}
