using Start.Scripts.Item;
using UnityEngine;
using UnityEngine.Events;

namespace Start.Scripts.Inventory
{
    public class InventoryHolder : MonoBehaviour
    {
        [SerializeField] protected InventorySystem _inventorySystem;
        [SerializeField] private int inventorySize;
        
        public InventorySystem InventorySystem => _inventorySystem;

        public static UnityAction<InventorySystem> OnDynamicInventoryDisplayRequested;

        private void Awake()
        {
            _inventorySystem = new InventorySystem(inventorySize);
        }
    }
}