using System.Collections.Generic;
using UnityEngine;

namespace Start.Scripts.Enemy
{
    /// <summary>
    /// Factory class for creating AI strategies based on enemy type.
    /// This centralizes strategy creation and allows for easier extensibility.
    /// </summary>
    public class AIStrategyFactory
    {
        // Cache of strategies to avoid creating new objects each time
        private static readonly Dictionary<string, IAIStrategy> StrategyCache = new Dictionary<string, IAIStrategy>();
        
        /// <summary>
        /// Get the appropriate AI strategy for a specific enemy type.
        /// </summary>
        /// <param name="enemyType">The type of enemy (Melee, Ranged, etc.)</param>
        /// <returns>An AI strategy appropriate for the enemy type</returns>
        public static IAIStrategy GetStrategy(string enemyType)
        {
            // Check if we already have a cached instance of this strategy
            if (StrategyCache.ContainsKey(enemyType))
            {
                return StrategyCache[enemyType];
            }
            
            // Create a new strategy based on enemy type
            IAIStrategy strategy = enemyType.ToLower() switch
            {
                "melee" => new MeleeAIStrategy(),
                "ranged" => new RangedAIStrategy(),
                "versatile" => new VersatileAIStrategy(), // You would need to implement this
                "magic" => new MagicAIStrategy(),         // You would need to implement this
                _ => new MeleeAIStrategy()                // Default to melee if type is unknown
            };
            
            // Cache the strategy for future use
            StrategyCache[enemyType] = strategy;
            
            return strategy;
        }
        
        /// <summary>
        /// Clear the strategy cache.
        /// Useful when changing scenes or when strategies need to be recreated.
        /// </summary>
        public static void ClearCache()
        {
            StrategyCache.Clear();
        }
    }
} 