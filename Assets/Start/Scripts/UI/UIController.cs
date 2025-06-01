using System.Collections.Generic;
using Start.Scripts.Character;
using Start.Scripts.Inventory;
using UnityEngine;
using Start.Scripts.Game;

namespace Start.Scripts.UI
{
    public class UIController : MonoBehaviour
    {
        public static UIController Instance { get; private set; }
        private GameManager _gameManager;
        [SerializeField] private StatDisplay statDisplay;
        [SerializeField] private InventoryDisplay inventoryDisplay;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            _gameManager = GameManager.Instance;
        }

        private void OnDestroy()
        {
            // TODO: Unsubscribe from events
        }

        private void UpdateStats(CharacterInfoData data)
        {
            statDisplay?.UpdateStats(data);
        }

        private void UpdateInventory(InventoryHolder inventory)
        {
            // inventoryDisplay?.UpdateInventory(inventory);
        }

        private void UpdatePartyUI(List<CharacterInfoData> party)
        {
            // TODO: Update party portrait panel, turn order UI, etc.
        }
    }
}

