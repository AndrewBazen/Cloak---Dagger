using System;
using Start.Scripts.Item;
using System.Collections;
using System.Collections.Generic;
using Start.Scripts.Inventory;
using SuperTiled2Unity;
using UnityEngine;


namespace Start.Scripts.UI
{
    public class StaticInventoryDisplay : InventoryDisplay
    {
        [SerializeField] private InventoryHolder _inventoryHolder;
        [SerializeField] private InventorySlotUI[] slots;

        
        private void Update()
        {
            inventorySystem = _inventoryHolder.InventorySystem;
            inventorySystem.OnInventorySlotChanged += UpdateSlot;
            AssignSlot(inventorySystem);
        }

        public override void AssignSlot(InventorySystem invToDisplay)
        {
            slotDictionary = new Dictionary<InventorySlotUI, InventorySlot>();
            for (int i = 0; i < inventorySystem.InventorySize; i++)
            {
                slotDictionary.Add(slots[i], inventorySystem.Inventory[i]);
                slots[i].Init(inventorySystem.Inventory[i]);
            }
        }
    }
}