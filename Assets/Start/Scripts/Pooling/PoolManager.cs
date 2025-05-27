using System.Collections.Generic;
using Start.Scripts.Enemy;
using UnityEngine;

namespace Start.Scripts.Pooling
{
    /// <summary>
    /// Singleton manager for all object pools in the game.
    /// Provides centralized access to pools for common game objects.
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        private static PoolManager _instance;
        public static PoolManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("PoolManager");
                    _instance = go.AddComponent<PoolManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // Container for all pools
        private readonly Dictionary<string, object> _pools = new Dictionary<string, object>();

        // Root transform for better hierarchy organization
        private Transform _poolsRoot;

        [Header("Prefabs for Pooling")]
        [SerializeField] private List<PooledPrefabDefinition> pooledPrefabs = new List<PooledPrefabDefinition>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            _poolsRoot = new GameObject("Pools").transform;
            _poolsRoot.SetParent(transform);

            // Initialize pools from prefab definitions
            InitializePools();
        }

        private void InitializePools()
        {
            foreach (PooledPrefabDefinition definition in pooledPrefabs)
            {
                if (definition.prefab != null)
                {
                    //  CreatePool(definition.prefab, definition.initialSize, definition.maxSize);
                }
                else
                {
                    Debug.LogWarning("Pooled prefab is null! Skipping pool creation.");
                }
            }
        }

        /// <summary>
        /// Creates a new pool for the specified prefab type
        /// </summary>
        public ObjectPool<T> CreatePool<T>(T prefab, int initialSize = 10, int maxSize = 50) where T : Component
        {
            string key = GetKeyForPrefab(prefab);
            if (_pools.ContainsKey(key))
            {
                Debug.LogWarning($"Pool for {key} already exists!");
                return (ObjectPool<T>)_pools[key];
            }

            // Create a container for this specific pool
            var poolContainer = new GameObject($"{typeof(T).Name}Pool").transform;
            poolContainer.SetParent(_poolsRoot);
            var pool = new ObjectPool<T>(prefab, poolContainer, initialSize, maxSize);
            _pools.Add(key, pool);

            // Pre-populate the pool
            var tempList = new List<T>();
            for (int i = 0; i < initialSize; i++)
            {
                var item = pool.Get();
                tempList.Add(item);
            }

            // Return them all to the pool
            foreach (var item in tempList)
            {
                pool.Release(item);
            }
            return pool;
        }

        /// <summary>
        /// Gets an existing pool for the specified prefab type
        /// </summary>
        public ObjectPool<T> GetPool<T>(T prefab) where T : Component
        {
            string key = GetKeyForPrefab(prefab);
            if (_pools.TryGetValue(key, out var pool))
            {
                return (ObjectPool<T>)pool;
            }
            return CreatePool(prefab);
        }

        /// <summary>
        /// Get or create a pool for an enemy type
        /// </summary>
        public ObjectPool<EnemyController> GetEnemyPool(EnemyController enemyPrefab)
        {
            return GetPool(enemyPrefab);
        }

        /// <summary>
        /// Release all pools and destroy all pooled objects
        /// </summary>
        public void ReleaseAllPools()
        {
            foreach (var pool in _pools.Values)
            {
                var clearMethod = pool.GetType().GetMethod("Clear");
                if (clearMethod != null)
                {
                    clearMethod.Invoke(pool, null);
                }
            }
            _pools.Clear();
        }

        /// <summary>
        /// Generate a unique key for a prefab
        /// </summary>
        private string GetKeyForPrefab<T>(T prefab) where T : Component
        {
            return $"{prefab.GetType().Name}_{prefab.name}";
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                ReleaseAllPools();
                _instance = null;
            }
        }
    }

    /// <summary>
    /// Definition for prefabs that should be pre-pooled on startup
    /// </summary>
    [System.Serializable]
    public class PooledPrefabDefinition
    {
        public GameObject prefab;
        public int initialSize = 10;
        public int maxSize = 50;
    }
}
