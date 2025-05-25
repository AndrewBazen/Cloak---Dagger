// StrategyFactory.cs
using System;
using System.Collections.Generic;
using Start.Scripts.Enemy;

namespace Start.Scripts.AI
{
    public class StrategyFactory
    {
        private readonly Dictionary<string, Func<IAIStrategy>> _registry = new();

        public void Register(string key, Func<IAIStrategy> constructor)
        {
            if (_registry.ContainsKey(key))
            {
                UnityEngine.Debug.LogWarning($"Strategy key '{key}' is already registered. Overwriting.");
            }
            _registry[key] = constructor;
        }

        public IAIStrategy Create(string key)
        {
            if (_registry.TryGetValue(key, out var constructor))
            {
                return constructor();
            }

            UnityEngine.Debug.LogError($"Strategy key '{key}' not found in registry.");
            return null;
        }

        public IEnumerable<string> GetAvailableStrategies() => _registry.Keys;
    }
}

