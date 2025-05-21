using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Start.Scripts.Inventory
{
    [Serializable]
    public class InventorySystem
    {
        [SerializeField] private List<InventorySlot> _inventory;

        public List<InventorySlot> Inventory => _inventory;
        public int InventorySize => Inventory.Count;

        public UnityAction<InventorySlot> OnInventorySlotChanged;

        public InventorySystem(int size)
        {
            _inventory = new List<InventorySlot>(size);

            for (int i = 0; i < size; i++)
            {
                _inventory.Add(new InventorySlot());
            }
        }

        public bool AddToInventory(InventoryItemData itemToAdd, int amountToAdd)
        {
            if (ContainsItem(itemToAdd, out List<InventorySlot> invSlot))
            {
                foreach (var slot in invSlot)
                {
                    if (slot.RoomLeftInStack(amountToAdd))
                    {
                        slot.AddToStack(amountToAdd);
                        OnInventorySlotChanged?.Invoke(slot);
                        return true;
                    }
                }
                
            }
            if (HasFreeSlot(out InventorySlot freeSlot))
            {
                freeSlot.UpdateInventorySlot(itemToAdd, amountToAdd);
                OnInventorySlotChanged?.Invoke(freeSlot);
                return true;
            }

            return false;
        }

        public bool ContainsItem(InventoryItemData itemToAdd, out List<InventorySlot> invSlot)
        {
            invSlot = Inventory.Where(i => i.Data == itemToAdd).ToList();

            return invSlot != null;
        }

        public bool HasFreeSlot(out InventorySlot freeSlot)
        {
            freeSlot = Inventory.FirstOrDefault(i => i.Data == null);
            return freeSlot != null;
        }

    }
}