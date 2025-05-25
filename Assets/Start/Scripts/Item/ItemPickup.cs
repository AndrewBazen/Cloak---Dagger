using Start.Scripts.Inventory;
using Start.Scripts.Character;
using TMPro;
using UnityEngine;

namespace Start.Scripts.Item
{
    public class ItemPickup : MonoBehaviour
    {
        public InventoryItemData ItemData;
        private GameObject _itemText;
        public GameObject textPrefab;

        private void OnTriggerEnter2D(Collider2D col)
        {
            var inventory = col.transform.GetComponent<PlayerController>().characterData.inventory;

            if (!inventory) return;

            if (inventory.InventorySystem.AddToInventory(ItemData, 1))
            {
                _itemText = Instantiate(textPrefab, col.gameObject.transform);
                _itemText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText($"+1 {ItemData.itemName}");
                Destroy(gameObject);
            }
        }
    }
}
