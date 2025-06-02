using System.Collections.Generic;
using UnityEngine;
using Start.Scripts.Enemy;
using Start.Scripts.Game;
using Start.Scripts.Inventory;
using Start.Scripts.UI;
using Start.Scripts.Combat;
using Start.Scripts.BaseClasses;

namespace Start.Scripts.Character
{
    public partial class PlayerController : Actor
    {
        [Header("References")]
        public GameObject Cursor;
        public CharacterInfoData characterData;
        public GameObject PlayerContainer => playerContainer;
        public GameObject EnemyContainer => enemyContainer;
        public GameObject InventoryContainer => inventoryContainer;

        [SerializeField] private GameObject playerContainer;
        [SerializeField] private GameObject enemyContainer;
        [SerializeField] private GameObject inventoryContainer;

        // Runtime state formerly from CharacterInfo
        private StatDisplay statDisplay;

        [Header("State")]
        private EnemyController _selectedEnemy;
        private List<OverlayTile> _rangeFinderTiles;
        private OverlayTile _overlayTile;
       private static Camera _camera;


        public PlayerController()
        {
            // WARNING: Unity does not guarantee constructor execution like regular C# classes
        }

        private void Awake()
        {
            _camera = Camera.main;
            InitializeComponents();
            SubscribeToEvents();
        }

        public void Initialize(CharacterInfoData characterData)
        {
            this.characterData = characterData;
        }

        protected override void Start()
        {
            if (characterData == null)
            {
                Debug.LogError("Character data is not set. Please assign a CharacterInfoData object.");
                return;
            }

            new WaitForEndOfFrame();

            if (_gameManager == null && GameManager.Instance != null)
            {
                if (statDisplay != null)
                {
                    statDisplay.UpdateStats(characterData);
                }
            }
        }

        private void InitializeComponents()
        {
            _combatController = GetComponent<CombatController>();
            if (_combatController == null)
            {
                _combatController = gameObject.AddComponent<CombatController>();
            }
        }
    }
}

