using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Start.Scripts.Dice;
using Start.Scripts.Enemy;
using Start.Scripts.Game;
using Start.Scripts.Inventory;
using Start.Scripts.UI;
using Start.Scripts.Combat;
using System.Linq;

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
        public int Initiative
        {
            get => initiative;
            set
            {
                initiative = value;
                OnPropertyChanged(nameof(Initiative));
            }
        }
        public List<GameObject> Enemies
        {
            get => _enemies;
            set => _enemies = value;
        }
        public OverlayTile StandingOnTile
        {
            get => standingOnTile;
            set
            {
                standingOnTile = value;
                OnPropertyChanged(nameof(StandingOnTile));
            }
        }
        public int CurrentHealth
        {
            get => currentHealth;
            set
            {
                currentHealth = value;
                OnPropertyChanged(nameof(CurrentHealth));
            }
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
        private static Camera _camera;
        private GameManager _gameManager;

        public event PropertyChangedEventHandler PropertyChanged;

        public PlayerController()
        {
            // WARNING: Unity does not guarantee constructor execution like regular C# classes
        }

        void IGameManagerAware.Initialize(GameManager gameManager)
        {
            throw new System.NotImplementedException();
        }


        private void Awake()
        {
            _camera = Camera.main;
            _enemies = new List<GameObject>();

            InitializeComponents();
            LoadCharacterStats();
            InitializePlayer();
            SubscribeToEvents();
        }

        private void Start()
        {
            if (characterData == null)
            {
                Debug.LogError("Character data is not set. Please assign a CharacterInfoData object.");
                return;
            }

            new WaitForEndOfFrame();

            if (_gameManager == null && GameManager.Instance != null)
            {
                Initialize(GameManager.Instance);
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

            _camera = Camera.main;
        }

        public void OnGameStateChanged(GameManager.GameState newState)
        {
            //update game state logic here if needed
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

            RollInitiative();
        }

        private void Initialize(GameManager gameManager)
        {
            _gameManager = gameManager;
            if (_gameManager != null)
            {
                _gameManager.Party.AddToParty(this.gameObject);
                _gameManager.Party.OnCharacterStatsChanged += (stats) => characterData.stats = stats;
                _gameManager.Party.OnCharacterInventoryChanged += (inventory) => characterData.inventory = inventory;
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
            var initiativeRoll = diceRoll.RollDice("D20", 1).Keys.FirstOrDefault();
            initiative = initiativeRoll;
        }
    }
}

