using System;
using System.Collections.Generic;
using Start.Scripts.Inventory;
using UnityEngine;

namespace Start.Scripts.Game
{
    public class GameEvents : MonoBehaviour
    {
        public static GameEvents current { get; private set; }

        private void Awake()
        {
            if (current != null && current != this)
            {
                Destroy(gameObject);
                return;
            }
            current = this;
            DontDestroyOnLoad(gameObject);
        }

        // ───── General Game Events ─────
        public event Action OnLoadEvent;
        public event Action OnEscPressed;
        public event Action OnGameOver;
        public event Action OnLoadNewScene;

        // ───── Player Events ─────
        public event Action OnPlayerSpawn;
        public event Action OnPlayerDeath;
        public event Action OnPlayerLevelUp;
        public event Action OnPlayerAttack;
        public event Action OnPlayerDamaged;
        public event Action OnPlayerUseItem;
        public event Action OnPlayerUseAbility;

        // ───── Enemy Events ─────
        public event Action OnEnemySpawn;
        public event Action OnEnemyDeath;
        public event Action OnEnemyAttack;
        public event Action OnEnemyDamaged;
        public event Action OnEnemyUseAbility;
        public event Action OnEnemyUseItem;

        // ───── Inventory Events ─────
        public event Action OnInventoryOpen;
        public event Action OnInventoryClose;
        public event Action OnInventoryItemAdded;
        public event Action OnInventoryItemRemoved;

        // ───── UI & Party Events ─────
        public event Action OnSubmitStats;
        public event Action OnSubmitParty;

        // ───── Turn-Based Events ─────
        public event Action<Action, Action> OnTurnStart;
        public event Action<Action, Action> OnTurnEnd;
        public event Action<List<GameObject>> OnEnemiesChanged;
        public event Action<InventoryHolder> OnInventoryUpdate;

        // ───── Public Trigger Methods ─────
        public void TriggerLoadEvent() => OnLoadEvent?.Invoke();
        public void TriggerGameOver() => OnGameOver?.Invoke();
        public void TriggerEscPressed() => OnEscPressed?.Invoke();
        public void TriggerSceneLoad() => OnLoadNewScene?.Invoke();
        public void TriggerEnemySpawn() => OnEnemySpawn?.Invoke();
        public void TriggerEnemyListUpdate(List<GameObject> enemies) => OnEnemiesChanged?.Invoke(enemies);
        public void TriggerInventoryUpdate(InventoryHolder inventory) => OnInventoryUpdate?.Invoke(inventory);
    }
}
