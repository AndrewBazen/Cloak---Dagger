using System.Collections.Generic;
using System.Linq;
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
    public class PlayerController : MonoBehaviour, INotifyPropertyChanged, IGameManagerAware
    {
        [Header("References")]
        public GameObject Cursor;
        public CharacterInfo Player;
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

        [Header("Settings")]
        public float speed;
        private const int CharacterMovementRange = 6;  // TODO: need to be based on the players class, race, and other factors

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

        #region GameManagerAware Implementation
        public void Initialize(GameManager gameManager)
        {
            _gameManager = gameManager;
            
            // Subscribe to game manager events
            _gameManager.OnGameOver += OnGameOver;
            _gameManager.OnCombatStarted += OnCombatStarted;
            _gameManager.OnCombatEnded += OnCombatEnded;
            
            // Register this player with GameManager
            if (Player != null)
            {
                _gameManager.AddToParty(Player);
                
                // If this is the first player, set as active
                if (_gameManager.ActivePlayer == null)
                {
                    _gameManager.SetActivePlayer(Player);
                }
            }
        }
        
        public void OnGameStateChanged(GameManager.GameState newState)
        {
            // Handle state transitions
            switch (newState)
            {
                case GameManager.GameState.Combat:
                    // Combat preparation
                    break;
                case GameManager.GameState.Exploration:
                    // Exploration setup
                    Player.hasMovement = true;
                    break;
                case GameManager.GameState.Paused:
                    // Pause handling
                    break;
            }
        }
        
        private void OnGameOver()
        {
            // Handle game over event
            Debug.Log("Game Over");
        }
        
        private void OnCombatStarted()
        {
            // Handle combat start
            Debug.Log("Combat Started");
        }
        
        private void OnCombatEnded()
        {
            // Handle combat end
            Debug.Log("Combat Ended");
            Player.hasMovement = true;
            Player.hasAttack = true;
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events when destroyed
            if (_gameManager != null)
            {
                _gameManager.OnGameOver -= OnGameOver;
                _gameManager.OnCombatStarted -= OnCombatStarted;
                _gameManager.OnCombatEnded -= OnCombatEnded;
            }
        }
        #endregion

        public PlayerController() {
            _pathFinder = new PathFinder();
            _rangeFinder = new RangeFinder();
            _camera = Camera.main;
            _enemies = new List<GameObject>();
        }

        private void Awake()
        {
            InitializeComponents();
            InitializePlayer();
            SubscribeToEvents();
        }

        private async void Start() {
            if (Player == null) return;
            await new WaitForEndOfFrame();
            Player.statDisplay?.UpdateStats(Player);
            Player.statDisplay?.UpdateInventory(Player.inventory);
            
            // Register with GameManager if not explicitly initialized
            if (_gameManager == null && GameManager.Instance != null)
            {
                Initialize(GameManager.Instance);
            }
        }

        private void InitializeComponents()
        {
            _combatController = GetComponent<CombatController>();
            if (_combatController == null)
            {
                _combatController = gameObject.AddComponent<CombatController>();
            }
            
            _pathFinder = new PathFinder();
            _rangeFinder = new RangeFinder();
            _camera = Camera.main;
        }

        private void InitializePlayer()
        {
            Player = GetComponent<CharacterInfo>();
            if (Player == null || string.IsNullOrEmpty(Player.id)) return;

            var statController = new StatController();
            statController.UpdateStats(Player, Player.skills);

            inventoryContainer = GameObject.FindGameObjectWithTag("inventory");
            Player.inventory = inventoryContainer?.GetComponent<InventoryHolder>();

            enemyContainer = GameObject.FindGameObjectWithTag("Enemies");
            playerContainer = GameObject.FindGameObjectWithTag("Players");
            Cursor = GameObject.FindGameObjectWithTag("cursor");

            // Use GameManager for initiative or roll it directly
            if (_gameManager != null)
            {
                _gameManager.RollInitiative(Player);
            }
            else
            {
                RollInitiative();
            }
        }

        public void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SubscribeToEvents()
        {
            if (GameEvents.current != null)
            {
                GameEvents.current.OnLoadEvent += DestroyMe;
                GameEvents.current.OnTurnStart += (_combatController.StartTurn, GetInRangeTiles);
                GameEvents.current.OnTurnEnd += (_combatController.StopTurn, ResetTiles);
                GameEvents.current.OnInventoryUpdate += (inventory) => Player.inventory = inventory;
                GameEvents.current.OnEnemiesChanged += (enemies) => _enemies = enemies;
                GameEvents.current.BroadcastMessage();
            }
            else
            {
                Debug.LogWarning("GameEvents.current is null. Event subscriptions skipped.");
            }
        }

        private void DestroyMe()
        {
            if (GameEvents.current != null)
            {
                GameEvents.current.OnLoadEvent -= DestroyMe;
            }
            
            // Remove from GameManager if registered
            if (_gameManager != null && Player != null)
            {
                _gameManager.RemoveFromParty(Player);
            }
            
            Destroy(gameObject);
        }

        private void Update()
        {
            if (_combatController == null || !_combatController.isTurn) return;

            GetInRangeTiles();
            HandleCursorMovement();

            if (Player.hasMovement)
            {
                HandleMovement();
            }
            else if (Player.hasAttack)
            {
                HandleAttack();
            }
        }

        private void HandleCursorMovement()
        {
            var hit = GetFocusedOnTile();
            if (hit.HasValue && hit.Value.collider != null && hit.Value.collider.gameObject.CompareTag("OverlayTile"))
            {
                _overlayTile = hit.Value.collider.gameObject.GetComponent<OverlayTile>();
                if (Cursor != null && _overlayTile != null)
                {
                    Cursor.transform.position = _overlayTile.transform.position;
                }
            }
        }

        private void HandleMovement()
        {
            if (!_isMoving && _overlayTile != null && Player.standingOnTile != null)
            {
                _path = _pathFinder.FindPath(Player.standingOnTile, _overlayTile, _rangeFinderTiles, false);
                HighlightPath();
            }

            MoveCharacter(_overlayTile);
        }

        private void HandleAttack()
        {
            var hit = GetFocusedOnTile();
            if (hit.HasValue && hit.Value.collider != null && hit.Value.collider.gameObject.CompareTag("enemy"))
            {
                _selectedEnemy = hit.Value.collider.gameObject.GetComponent<EnemyController>();
                if (Cursor != null && _selectedEnemy != null)
                {
                    Cursor.transform.position = new Vector3(_selectedEnemy.transform.position.x, _selectedEnemy.transform.position.y + 1, _selectedEnemy.transform.position.z);
                }

                if (Input.GetMouseButtonDown(0) && _selectedEnemy && EnemyInWeaponRange())
                {
                    AttackEnemy();
                }
            }
        }

        private void HighlightPath()
        {
            if (_path == null) return;
            
            foreach (var tile in _path)
            {
                if (tile == null) continue;
                
                if (tile != _overlayTile)
                    tile.SetSprite();
                tile.ShowTile();
            }
            
            _overlayTile?.ShowTile();
        }

        private void MoveCharacter(OverlayTile tile)
        {
            if (Input.GetMouseButtonDown(0) && tile != null && !tile.isBlocked && _rangeFinderTiles.Contains(tile))
            {
                _isMoving = true;
                ResetTiles();
            }

            if (_path != null && _path.Count > 0 && _isMoving)
            {
                MoveAlongPath();
            }
        }

        private void MoveAlongPath()
        {
            var step = speed * Time.deltaTime;
            var zIndex = _path[0].transform.position.z;

            Player.transform.position = Vector2.MoveTowards(Player.transform.position, _path[0].transform.position, step);
            Player.transform.position = new Vector3(Player.transform.position.x, Player.transform.position.y, zIndex);

            if (Vector2.Distance(Player.transform.position, _path[0].transform.position) < 0.00001f)
            {
                PositionCharacterOnTile(_path[0]);
                _path.RemoveAt(0);
            }

            if (_path.Count == 0)
            {
                _isMoving = false;
                Player.hasMovement = false;
                GetInRangeTiles();
                
                // Notify GameManager of player movement
                if (_gameManager != null && Player.standingOnTile != null)
                {
                    _gameManager.OnPlayerMoved?.Invoke(Player.standingOnTile.gridLocation);
                }
            }
        }

        private void PositionCharacterOnTile(OverlayTile tile)
        {
            if (tile == null) return;
            
            var tilePos = tile.transform.position;
            Player.transform.position = new Vector3(tilePos.x, tilePos.y + 0.0001f, tilePos.z);
            Player.standingOnTile = tile;
        }

        private void AttackEnemy()
        {
            if (_selectedEnemy == null || _combatController == null) return;
            
            _combatController.AttackOtherCharacter(Player, _selectedEnemy);
            Player.hasAttack = false;
        }

        private void ResetTiles()
        {
            if (MapManager.Instance == null || MapManager.Instance.Map == null) return;
            
            foreach (var overlayTile in MapManager.Instance.Map.Values)
            {
                if (overlayTile == null) continue;
                
                if (!_rangeFinderTiles.Contains(overlayTile) && !_path.Contains(overlayTile))
                {
                    overlayTile.HideTile();
                }
                if (!_path.Contains(overlayTile))
                {
                    overlayTile.ResetSprite();
                }
            }
        }

        private void GetInRangeTiles()
        {
            if (Player == null || Player.standingOnTile == null) return;
            
            _rangeFinderTiles = _rangeFinder.GetTilesInRange(
                new Vector2Int(Player.standingOnTile.gridLocation.x, Player.standingOnTile.gridLocation.y),
                CharacterMovementRange);
        }

        private static RaycastHit2D? GetFocusedOnTile()
        {
            if (Camera.main == null) return null;
            
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos2D, Vector2.zero);

            return hits.Length > 0 ? hits[0] : (RaycastHit2D?)null;
        }

        private bool EnemyInWeaponRange()
        {
            if (Player == null || Player.standingOnTile == null || _selectedEnemy == null || _selectedEnemy.standingOnTile == null)
                return false;
                
            var distance = _pathFinder.FindPath(Player.standingOnTile, _selectedEnemy.standingOnTile,
                new List<OverlayTile>(), false);
                
            if (distance == null || Player.weapon == null)
                return false;
                
            return distance.Count <= Player.weapon.range;
        }

        private void RollInitiative()
        {
            var diceRoll = new DiceRoll();
            var initiativeRoll = diceRoll.RollDice("D20", 1).Keys.First();
            Player.initiative = initiativeRoll;
        }
    }
} 