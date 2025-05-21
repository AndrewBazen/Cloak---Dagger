using System;
using UnityEngine;

namespace Start.Scripts.Inventory
{
    [Serializable]
    public class InventorySlot
    {
        [SerializeField] private InventoryItemData _data;
        [SerializeField] private int _stackSize;
        
        public InventoryItemData Data => _data;

        public int StackSize => _stackSize;

        public InventorySlot(InventoryItemData source, int amount)
        {
            _data = source;
            _stackSize = amount;
        }
        
        public InventorySlot()
        {
            ClearSlot();
        }

        public void ClearSlot()
        {
            _data = null;
            _stackSize = -1;
        }

        public void UpdateInventorySlot(InventoryItemData data, int amount)
        {
            _data = data;
            _stackSize = amount;
        }

        public bool RoomLeftInStack(int amountToAdd, out int amountRemaining)
        {
            amountRemaining = Data.maxStackSize - _stackSize;

            return RoomLeftInStack(amountToAdd);
        }
        
        public bool RoomLeftInStack(int amountToAdd)
        {
            if (_stackSize + amountToAdd <= _data.maxStackSize) return true;
            return false;
        }

        public void AddToStack(int amount)
        {
            _stackSize += amount;
        }

        public void RemoveFromStack(int amount)
        {
            _stackSize -= amount;
        }
    }
}