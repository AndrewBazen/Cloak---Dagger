using UnityEngine;

namespace Start.Scripts.Enemy
{
    /// <summary>
    /// Interface for different enemy AI strategies.
    /// Implementations of this interface define how different types of enemies make decisions.
    /// </summary>
    public interface IAIStrategy
    {
        /// <summary>
        /// Evaluates the current game state and determines the best strategy for the enemy.
        /// </summary>
        /// <param name="enemy">The enemy controller this strategy is working for</param>
        /// <param name="gameManager">Reference to the game manager for accessing game state</param>
        /// <returns>A Strategy object containing the decision information</returns>
        Strategy EvaluateStrategy(EnemyController enemy, Game.GameManager gameManager);
    }
} 