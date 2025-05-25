
namespace Start.Scripts.Enemy.Strategy
{
    public static class AIStrategyRegistrar
    {
        private static bool _initialized;

        public static void RegisterAll()
        {
            if (_initialized) return;
            _initialized = true;

            StrategyFactory.Register("Melee", () => new MeleeAIStrategy());
            StrategyFactory.Register("Ranged", () => new RangedAIStrategy());
            StrategyFactory.Register("Magic", () => new MagicAIStrategy());
            // Add more as needed
        }
    }
}

