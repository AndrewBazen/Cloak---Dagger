using System;
using UnityEngine;

namespace Start.Scripts.Pooling
{
    /// <summary>
    /// Component attached to pooled objects to track pool ownership and handle lifecycle events.
    /// </summary>
    public class PoolableObject : MonoBehaviour
    {
        // Reference to the owning pool (set through reflection)
        public object Pool { get; set; }
        
        // Event triggered when object is obtained from pool
        public event Action<PoolableObject> OnActivated;
        
        // Event triggered when object is returned to pool
        public event Action<PoolableObject> OnDeactivated;
        
        /// <summary>
        /// Called when the object is retrieved from the pool
        /// </summary>
        public virtual void OnTakeFromPool()
        {
            OnActivated?.Invoke(this);
        }
        
        /// <summary>
        /// Called when the object is returned to the pool
        /// </summary>
        public virtual void OnReturnToPool()
        {
            OnDeactivated?.Invoke(this);
        }
        
        /// <summary>
        /// Release this object back to its pool
        /// </summary>
        public void Release()
        {
            if (Pool == null)
            {
                Debug.LogWarning("Trying to release an object that is not pooled");
                return;
            }
            
            // Use reflection to call the Release method on the pool
            var releaseMethod = Pool.GetType().GetMethod("Release", new[] { typeof(PoolableObject) });
            if (releaseMethod != null)
            {
                releaseMethod.Invoke(Pool, new object[] { this });
            }
            else
            {
                Debug.LogError($"Pool of type {Pool.GetType().Name} does not have a Release(PoolableObject) method");
            }
        }
    }
} 