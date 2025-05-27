
namespace Start.Scripts.Enemy.Strategies
{
    public static class AIStrategyRegistrar
    {
        private static bool _initialized;

        public static void RegisterAll()
        {
            if (_initialized) return;
            _initialized = true;

            AIStrategyFactory.Register("Melee", () => new MeleeAIStrategy());
            AIStrategyFactory.Register("Ranged", () => new RangedAIStrategy());
            AIStrategyFactory.Register("Magic", () => new MagicAIStrategy());
            // Add more as needed
        }
    }
}

