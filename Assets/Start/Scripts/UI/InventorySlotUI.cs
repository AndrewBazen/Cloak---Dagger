using System;
using Start.Scripts.Inventory;
using Start.Scripts.Item;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

namespace Start.Scripts.UI
{
    public class InventorySlotUI : MonoBehaviour
    {
        [SerializeField] private Image itemSprite;
        [SerializeField] private TextMeshProUGUI itemCount;
        [SerializeField] private InventorySlot assignedInventorySlot;

        private Button _button;

        public InventorySlot AssignedInventorySlot => assignedInventorySlot;

        public InventoryDisplay ParentDisplay { get; private set; }

        public void Awake()
        {
            ClearSlot();

            _button = GetComponent<Button>();
            _button?.onClick.AddListener(OnUISlotClick);

            ParentDisplay = transform.parent.GetComponent<InventoryDisplay>();
        }

        public void Init(InventorySlot slot)
        {
            assignedInventorySlot = slot;
            UpdateUISlot(slot);
        }

        public void UpdateUISlot(InventorySlot slot)
        {
            if (slot.Data)
            {
                itemSprite.sprite = slot.Data.icon;
                itemSprite.color = Color.white;
                
                
                itemCount.text = slot.StackSize > 1 ? slot.StackSize.ToString() : "";
            }
            else
            {
                ClearSlot();
            }
        }

        public void UpdateUISlot()
        {
            if(assignedInventorySlot != null) UpdateUISlot(assignedInventorySlot);
        }

        private void ClearSlot()
        {
            assignedInventorySlot?.ClearSlot();
            itemSprite.sprite = null;
            itemSprite.color = Color.clear;
            itemCount.text = "";
        }

        private void OnUISlotClick()
        {
            ParentDisplay?.SlotClicked(this);
        }
    }
}