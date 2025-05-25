using System;
using System.Collections.Generic;
using UnityEngine;

namespace Start.Scripts.Enemy
{
    /// <summary>
    /// Factory and registry for managing AI strategy types.
    /// </summary>
    public static class AIStrategyFactory
    {
        private static readonly Dictionary<string, Func<IAIStrategy>> Registry = new();
        private static readonly Dictionary<string, IAIStrategy> Cache = new();

        // Configurable fallback strategy
        private static string _fallbackStrategyKey = "melee";

        static AIStrategyFactory()
        {
            // Load strategies from configuration or register defaults
            RegisterDefaults();
        }

        private static void RegisterDefaults()
        {
            var defaultStrategies = new Dictionary<string, Func<IAIStrategy>>
            {
                {"melee", () => new MeleeAIStrategy()},
                {"ranged", () => new RangedAIStrategy()},
                {"versatile", () => new VersatileAIStrategy()},
                {"magic", () => new MagicAIStrategy()}
            };

            foreach (var kvp in defaultStrategies)
            {
                Register(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// Registers a new strategy by type name.
        /// </summary>
        public static void Register(string enemyType, Func<IAIStrategy> factory)
        {
            var key = enemyType.ToLower();
            if (!Registry.ContainsKey(key))
            {
                Registry[key] = factory;
            }
        }

        /// <summary>
        /// Sets the default fallback strategy to use if a type is unknown.
        /// </summary>
        public static void SetFallbackStrategy(string fallbackKey)
        {
            _fallbackStrategyKey = fallbackKey.ToLower();
        }

        /// <summary>
        /// Gets a strategy instance for a given enemy type.
        /// </summary>
        public static IAIStrategy GetStrategy(string enemyType)
        {
            var key = enemyType.ToLower();

            if (Cache.TryGetValue(key, out var strategy))
                return strategy;

            if (Registry.TryGetValue(key, out var factory))
            {
                strategy = factory();
                Cache[key] = strategy;
                return strategy;
            }

            Debug.LogWarning($"Unknown AI strategy type: {enemyType}. Defaulting to {_fallbackStrategyKey}.");
            return GetStrategy(_fallbackStrategyKey);
        }

        /// <summary>
        /// Clears all cached strategies.
        /// </summary>
        public static void ClearCache()
        {
            Cache.Clear();
        }
    }
}
