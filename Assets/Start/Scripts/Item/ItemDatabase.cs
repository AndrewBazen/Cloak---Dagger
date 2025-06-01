// using Start.Scripts.Inventory;
// using System.Collections.Generic;
// using UnityEngine;
// using System.IO;


// namespace Start.Scripts.Item
// {
//     /// <summary>
//     /// Runtime-accessible database for all inventory items (weapons, armor, etc.).
//     /// </summary>
//     public class ItemDatabase : MonoBehaviour
//     {
//         public static ItemDatabase Instance { get; private set; }

//         [Header("Item Source")]
//         [SerializeField] private string resourceFolder = "InventoryItems";
//         private Dictionary<string, InventoryItemData> _itemLookup;

//         private void Awake()
//         {
//             if (Instance != null && Instance != this)
//             {
//                 Destroy(gameObject);
//                 return;
//             }

//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//             LoadItems();
//         }

//         private void LoadItems()
//         {
//             _itemLookup = new Dictionary<string, InventoryItemData>();
//             var items = Resources.LoadAll<InventoryItemData>(resourceFolder);

//             foreach (var item in items)
//             {
//                 if (item == null || string.IsNullOrEmpty(item.id)) continue;

//                 if (_itemLookup.ContainsKey(item.id))
//                 {
//                     Debug.LogWarning($"Duplicate item ID detected: {item.id}. Skipping.");
//                     continue;
//                 }

//                 _itemLookup[item.id] = item;
//             }

//             Debug.Log($"[ItemDatabase] Loaded {_itemLookup.Count} items from '{resourceFolder}'.");
//         }

//         public InventoryItemData GetItemById(string id)
//         {
//             return _itemLookup.TryGetValue(id, out var item) ? item : null;
//         }

//         public IEnumerable<InventoryItemData> GetAllItems() => _itemLookup.Values;
//     }
// }


