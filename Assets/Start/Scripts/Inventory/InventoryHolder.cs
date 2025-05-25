using UnityEngine;
using UnityEngine.Events;

namespace Start.Scripts.Inventory
{
    public class InventoryHolder : MonoBehaviour
    {
        [SerializeField] protected InventorySystem _inventorySystem;

        public InventorySystem InventorySystem => _inventorySystem;
        public int inventorySize = 20;

        public static UnityAction<InventorySystem> OnDynamicInventoryDisplayRequested;

        private void Awake()
        {
            _inventorySystem = new InventorySystem(inventorySize);
        }
    }
}
