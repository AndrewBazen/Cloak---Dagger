
using System.Collections.Generic;
using Start.Scripts.Inventory;
using Start.Scripts.Item;
using UnityEngine;


namespace Start.Scripts.UI
{
    public abstract class InventoryDisplay : MonoBehaviour
    {
        protected InventorySystem inventorySystem;
        protected Dictionary<InventorySlotUI, InventorySlot> slotDictionary;

        public InventorySystem InventorySystem => inventorySystem;
        public Dictionary<InventorySlotUI, InventorySlot> SlotDictionary => slotDictionary;

        protected virtual void Start()
        {
            
        }

        public abstract void AssignSlot(InventorySystem invToDisplay);

        protected virtual void UpdateSlot(InventorySlot updatedSlot)
        {
            foreach (var slot in slotDictionary)
            {
                if (slot.Value == updatedSlot) 
                {
                    slot.Key.UpdateUISlot(updatedSlot);
                }
            }
        }

        public void SlotClicked(InventorySlotUI clickedSlot)
        {
            Debug.Log("Slot Clicked");
        }


    }
}