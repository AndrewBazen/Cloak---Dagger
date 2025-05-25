using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Start.Scripts.Game;

namespace Start.Scripts.Enemy.Strategy
{
    /// <summary>
    /// AI strategy for magic-focused enemies that primarily use abilities.
    /// This is a placeholder implementation that can be expanded later.
    /// </summary>
    public class MagicAIStrategy : BaseAIStrategy
    {
        public override Strategy EvaluateStrategy(EnemyController enemy, GameManager gameManager)
        {
            var currentTile = enemy.standingOnTile;
            var bestStrategy = new Strategy();
            
            // Check if we have any abilities to use
            if (enemy.abilities.Count > 0 && enemy.mana > 0)
            {
                // Find the best ability to use
                var ability = FindBestAbility(enemy, gameManager);
                if (ability != null)
                {
                    // Find the best target for this ability
                    var targetInfo = FindBestAbilityTarget(enemy, ability, gameManager);
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
            return rangedStrategy.EvaluateStrategy(enemy, gameManager);
        }
        
        /// <summary>
        /// Finds the best ability to use based on current situation.
        /// This is a placeholder that should be expanded with real logic.
        /// </summary>
        private Ability FindBestAbility(EnemyController enemy, GameManager gameManager)
        {
          //TODO: adjust to account for friendly units and AOE abilities
            // Simple placeholder that just returns the first ability with enough mana cost
            return enemy.abilities.FirstOrDefault(a => a.manaCost <= enemy.mana);
        }
        
        /// <summary>
        /// Finds the best target for an ability and calculates its value.
        /// </summary>
        private (CharacterInfo, float) FindBestAbilityTarget(EnemyController enemy, Ability ability, GameManager gameManager)
        {
            // This is a placeholder implementation
            var abilityRange = ability.range > 0 ? ability.range : SpellRange;
            var playersInRange = GetPlayersInRange(enemy.standingOnTile, abilityRange, gameManager);
            
            if (playersInRange.Count == 0)
                return (null, -100000);
                
            // Just return the first player in range with a basic value
            var target = playersInRange[0];
            float value = 50.0f / (target.health + 10f); // Simple value calculation
            
            return (target, value);
        }
    }
} 
