
namespace Start.Scripts.Enemy.Strategies
{
    /// <summary>
    /// AI strategy for versatile enemies that can switch between melee and ranged attacks.
    /// This is a placeholder implementation that can be expanded later.
    /// </summary>
    public class VersatileAIStrategy : BaseAIStrategy
    {
        private readonly MeleeAIStrategy _meleeStrategy = new MeleeAIStrategy();
        private readonly RangedAIStrategy _rangedStrategy = new RangedAIStrategy();
        public override Strategy EvaluateStrategy(EnemyController enemy)
        {
            // Get strategies from both melee and ranged approaches
            var meleeStrategy = _meleeStrategy.EvaluateStrategy(enemy);
            var rangedStrategy = _rangedStrategy.EvaluateStrategy(enemy);

            // Compare the strategies and choose the better one
            if (meleeStrategy.StrategyValue > rangedStrategy.StrategyValue)
            {
                return meleeStrategy;
            }
            return rangedStrategy;
        }
    }
}
