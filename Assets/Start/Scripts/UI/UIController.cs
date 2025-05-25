using System.Collections.Generic;
using Start.Scripts.Character;
using Start.Scripts.Inventory;
using UnityEngine;

namespace Start.Scripts.UI
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private StatDisplay statDisplay;
        [SerializeField] private InventoryDisplay inventoryDisplay;

        private void Awake()
        {
            GameEvents.current.OnCharacterStatsChanged += UpdateStats;
            GameEvents.current.OnCharacterInventoryChanged += UpdateInventory;
            GameManager.Instance.OnPartyUpdated += UpdatePartyUI;
        }

        private void OnDestroy()
        {
            GameEvents.current.OnCharacterStatsChanged -= UpdateStats;
            GameEvents.current.OnCharacterInventoryChanged -= UpdateInventory;
            if (GameManager.Instance != null)
                GameManager.Instance.OnPartyUpdated -= UpdatePartyUI;
        }

        private void UpdateStats(CharacterInfoData data)
        {
            statDisplay?.UpdateStats(data);
        }

        private void UpdateInventory(InventoryHolder inventory)
        {
            inventoryDisplay?.UpdateInventory(inventory);
        }

        private void UpdatePartyUI(List<CharacterInfoData> party)
        {
            // TODO: Update party portrait panel, turn order UI, etc.
        }
    }
}

