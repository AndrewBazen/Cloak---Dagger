using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Start.Scripts.Pooling
{
    /// <summary>
    /// Generic object pooling class that manages reusable GameObject instances.
    /// Provides better performance by avoiding runtime instantiation and destruction of objects.
    /// </summary>
    /// <typeparam name="T">The component type to pool</typeparam>
    public class ObjectPool<T> where T : Component
    {
        // The prefab that will be instantiated
        private readonly T _prefab;
        
        // The ObjectPool from Unity's Pool package
        private readonly IObjectPool<T> _pool;
        
        // Default capacity and maximum size
        private const int DefaultCapacity = 10;
        private const int MaxSize = 50;
        
        // Track whether objects should be active when released
        private readonly bool _collectionCheck;
        
        // Parent transform to organize hierarchy
        private readonly Transform _parent;

        public IObjectPool<T> Pool => _pool;

        // Creates a new object pool with the specified prefab
        public ObjectPool(T prefab, Transform parent = null, int defaultCapacity = DefaultCapacity, int maxSize = MaxSize, bool collectionCheck = true)
        {
            _prefab = prefab;
            _parent = parent;
            _collectionCheck = collectionCheck;
            
            _pool = new UnityEngine.Pool.ObjectPool<T>(
                createFunc: CreatePooledItem,
                actionOnGet: OnTakeFromPool,
                actionOnRelease: OnReturnToPool,
                actionOnDestroy: OnDestroyPoolObject,
                collectionCheck: collectionCheck,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize);
        }
        
        // Get an object from the pool
        public T Get()
        {
            return _pool.Get();
        }
        
        // Release an object back to the pool
        public void Release(T element)
        {
            _pool.Release(element);
        }
        
        // Clear the pool, destroying all pooled objects
        public void Clear()
        {
            _pool.Clear();
        }
        
        // Create a new instance of the object
        private T CreatePooledItem()
        {
            var go = Object.Instantiate(_prefab, _parent);
            
            // Add a PoolableObject component if it doesn't exist
            if (go.GetComponent<PoolableObject>() == null)
            {
                go.gameObject.AddComponent<PoolableObject>().Pool = this;
            }
            
            return go;
        }
        
        // Called when an object is taken from the pool
        private void OnTakeFromPool(T pooledObject)
        {
            pooledObject.gameObject.SetActive(true);
            
            // Notify the poolable object
            var poolable = pooledObject.GetComponent<PoolableObject>();
            if (poolable != null)
            {
                poolable.OnTakeFromPool();
            }
        }
        
        // Called when an object is returned to the pool
        private void OnReturnToPool(T pooledObject)
        {
            pooledObject.gameObject.SetActive(false);
            
            // Notify the poolable object
            var poolable = pooledObject.GetComponent<PoolableObject>();
            if (poolable != null)
            {
                poolable.OnReturnToPool();
            }
        }
        
        // Called when an object is destroyed
        private void OnDestroyPoolObject(T pooledObject)
        {
            Object.Destroy(pooledObject.gameObject);
        }
        
        // Release an object via the PoolableObject component
        public void Release(PoolableObject poolableObject)
        {
            if (poolableObject.TryGetComponent(out T component))
            {
                Release(component);
            }
        }
    }
} 
