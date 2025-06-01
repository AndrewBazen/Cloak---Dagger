using System.Linq;
using Start.Scripts.Character;

namespace Start.Scripts.Enemy.Strategies
{
    /// <summary>
    /// AI strategy for magic-focused enemies that primarily use abilities.
    /// This is a placeholder implementation that can be expanded later.
    /// </summary>
    public class MagicAIStrategy : BaseAIStrategy
    {
        public override Strategy EvaluateStrategy(EnemyController enemy)
        {
            var currentTile = enemy.StandingOnTile;
            // Check if we have any abilities to use
            if (enemy.EnemyData.Abilities.Count > 0 && enemy.CurrentMana > 0)
            {
                // Find the best ability to use
                var ability = FindBestAbility(enemy);
                if (ability != null)
                {
                    // Find the best target for this ability
                    var targetInfo = FindBestAbilityTarget(enemy, ability);
                    if (targetInfo.Item1 != null)
                    {
                        return new Strategy(
                            currentTile,
                            currentTile,
                            targetInfo.Item1.gameObject,
                            ability,
                            targetInfo.Item2, // Attack value
                            0
                        );
                    }
                }
            }
            // If no good ability or not enough mana, fall back to ranged strategy
            var rangedStrategy = new RangedAIStrategy();
            return rangedStrategy.EvaluateStrategy(enemy);
        }
        /// <summary>
        /// Finds the best ability to use based on current situation.
        /// This is a placeholder that should be expanded with real logic.
        /// </summary>
        private Ability FindBestAbility(EnemyController enemy)
        {
            //TODO: adjust to account for friendly units and AOE abilities
            // Simple placeholder that just returns the first ability with enough mana cost
            return enemy.EnemyData.Abilities.FirstOrDefault(a => a.manaCost <= enemy.CurrentMana);
        }
        /// <summary>
        /// Finds the best target for an ability and calculates its value.
        /// </summary>
        private (PlayerController, float) FindBestAbilityTarget(EnemyController enemy, Ability ability)
        {
            // This is a placeholder implementation
            var abilityRange = ability.attackRange > 0 ? ability.attackRange : 2;
            var playersInRange = GetPlayersInRange(enemy.StandingOnTile, abilityRange);
            if (playersInRange.Count == 0)
                return (null, -100000);

            // Just return the first player in range with a basic value
            var target = playersInRange[0];
            float value = 50.0f / (target.CurrentHealth + 10f); // Simple value calculation
            return (target, value);
        }
    }
}
