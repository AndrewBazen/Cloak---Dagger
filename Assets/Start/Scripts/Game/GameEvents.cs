using System;
using System.Drawing;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Start.Scripts.Game
{
    public class GameEvents : MonoBehaviour
    {
        public static GameEvents current;
        public GameObject GameManager;

        private void Start()
        {
            current = this;
        }

        #region Game Events
        public event Action OnLoadEvent;
        public event Action OnCombatStart;
        public event Action OnGameOver;
        public event Action OnEscPressed;
        public event Action OnLoadNewScene;
        #endregion

        #region Player Events
        public event Action OnPlayerSpawn;
        public event Action OnPlayerDeath;
        public event Action OnPlayerLevelUp;
        public event Action OnPlayerAttack;
        public event Action OnPlayerDamaged;
        public event Action OnPlayerUseItem;
        public event Action OnPlayerUseAbility;
        #endregion

        #region Enemy Events
        public event Action OnEnemySpawn;
        public event Action OnEnemyDeath;
        public event Action OnEnemyAttack;
        public event Action OnEnemyDamaged;
        public event Action OnEnemyUseAbility;
        public event Action OnEnemyUseItem;
        #endregion

        #region Inventory Events
        public event Action OnInventoryOpen;
        public event Action OnInventoryClose;
        public event Action OnInventoryItemAdded;
        public event Action OnInventoryItemRemoved;
        #endregion

        #region UI Events
        public event Action OnSubmitStats;
        public event Action OnSubmitParty;
        #endregion

        #region Turn Events
        public event Action<Action, Action> OnTurnStart;
        public event Action<Action, Action> OnTurnEnd;
        public event Action<List<GameObject>> OnEnemiesChanged;
        public event Action<InventoryHolder> OnInventoryUpdate;
        #endregion

        public void BroadcastMessage()
        {
            // Method to broadcast messages or events
        }

        private void Update()
        {
            
        }

        public void LoadNewScene(string sceneName) {
            OnLoadNewScene?.Invoke();
            SceneManager.LoadScene(sceneName);
        }

        public void LoadEvent()
        {
            OnLoadEvent?.Invoke();
        }

        public void GameOver()
        {
            OnGameOver?.Invoke();
        }

        protected void OnOnLoadNewScene()
        {
            OnLoadNewScene?.Invoke();
        }

        protected void OnOnSubmitStats()
        {
            OnSubmitStats?.Invoke();
        }
    }
}