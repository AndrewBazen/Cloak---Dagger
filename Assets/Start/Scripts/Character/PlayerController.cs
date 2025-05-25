using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Start.Scripts.Dice;
using Start.Scripts.Enemy;
using Start.Scripts.Game;
using Start.Scripts.Inventory;
using Start.Scripts.UI;
using Start.Scripts.Map;
using Start.Scripts.Combat;

namespace Start.Scripts.Character
{
    public partial class PlayerController : MonoBehaviour, INotifyPropertyChanged, IGameManagerAware
    {
        [Header("References")]
        public GameObject Cursor;
        public CharacterInfoData characterData;
        public GameObject PlayerContainer => playerContainer;
        public GameObject EnemyContainer => enemyContainer;
        public GameObject InventoryContainer => inventoryContainer;
        public List<GameObject> Enemies {
            get => _enemies;
            set => _enemies = value;
        }

        [SerializeField] private GameObject playerContainer;
        [SerializeField] private GameObject enemyContainer;
        [SerializeField] private GameObject inventoryContainer;

        // Runtime state formerly from CharacterInfo
        private int currentHealth;
        private bool hasMovement;
        private bool hasAttack;
        private OverlayTile standingOnTile;
        private int initiative;
        private StatDisplay statDisplay;

        [Header("Settings")]
        public float speed;
        private const int CharacterMovementRange = 6;

        [Header("State")]
        private List<GameObject> _enemies;
        private EnemyController _selectedEnemy;
        private List<OverlayTile> _path;
        private List<OverlayTile> _rangeFinderTiles;
        private OverlayTile _overlayTile;
        private bool _isMoving;

        private CombatController _combatController;
        private PathFinder _pathFinder;
        private RangeFinder _rangeFinder;
        private static Camera _camera;
        private GameManager _gameManager;

        public event PropertyChangedEventHandler PropertyChanged;

        public PlayerController()
        {
            // WARNING: Unity does not guarantee constructor execution like regular C# classes
        }

        private void Awake()
        {
            _pathFinder = new PathFinder();
            _rangeFinder = new RangeFinder();
            _camera = Camera.main;
            _enemies = new List<GameObject>();

            InitializeComponents();
            LoadCharacterStats();
            InitializePlayer();
            SubscribeToEvents();
        }

        private async void Start()
        {
            if (characterData == null)
            {
                Debug.LogError("Character data is not set. Please assign a CharacterInfoData object.");
                return;
            }

            await new WaitForEndOfFrame();

            if (_gameManager == null && GameManager.Instance != null)
            {
                Initialize(GameManager.Instance);
                if (statDisplay != null)
                {
                    statDisplay.UpdateStats(characterData.stats);
                    statDisplay.UpdateInventory(characterData.inventory);
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

            _camera = Camera.main;
        }

        private void InitializePlayer()
        {
            if (characterData == null || string.IsNullOrEmpty(characterData.id)) return;

            var statController = new StatController();
            statController.UpdateStats(characterData, characterData.skills);

            inventoryContainer = GameObject.FindGameObjectWithTag("inventory");
            characterData.inventory = inventoryContainer?.GetComponent<InventoryHolder>();

            enemyContainer = GameObject.FindGameObjectWithTag("Enemies");
            playerContainer = GameObject.FindGameObjectWithTag("Players");
            Cursor = GameObject.FindGameObjectWithTag("cursor");

            if (_gameManager != null)
            {
                _gameManager.RollInitiative(characterData);
            }
            else
            {
                RollInitiative();
            }
        }

        private void LoadCharacterStats()
        {
            currentHealth = characterData.health;
            hasMovement = true;
            hasAttack = true;
            initiative = 0;
            standingOnTile = null;
        }

        private void RollInitiative()
        {
            var diceRoll = new DiceRoll();
            var initiativeRoll = diceRoll.RollDice("D20", 1).Keys.First();
            initiative = initiativeRoll;
        }
    }
}

