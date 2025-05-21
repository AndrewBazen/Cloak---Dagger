using System.Collections.Generic;
using System.Linq;
using Start.Scripts.Dice;
using Start.Scripts.Enemy;
using Start.Scripts.Game;
using Start.Scripts.Inventory;
using Start.Scripts.UI;
using UnityEngine;
using System.ComponentModel;

namespace Start.Scripts
{
    public class PlayerController : MonoBehaviour, INotifyPropertyChanged
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

        public event PropertyChangedEventHandler PropertyChanged;

        public PlayerController() {
            _pathFinder = new PathFinder();
            _rangeFinder = new RangeFinder();
            _camera = Camera.main;
            _combatController = new CombatController();
            _enemies = new List<GameObject>();
        }

        private void Awake()
        {
            InitializeComponents();
            InitializePlayer();
            SubscribeToEvents();
        }

        private async void Start() {
            if (player == null) return;
            await new WaitForEndOfFrame();
            player.statDisplay.UpdateStats(player);
            player.statDisplay.UpdateInventory(player.inventory);
        }

        private void InitializeComponents()
        {
            _combatController = GetComponent<CombatController>();
            _pathFinder = new PathFinder();
            _rangeFinder = new RangeFinder();
            _camera = Camera.main;
        }

        private void InitializePlayer()
        {
            player = GetComponent<CharacterInfo>();
            if (player == null || string.IsNullOrEmpty(player.id)) return;

            var statController = new StatController();
            statController.UpdateStats(player, player.skills);

            inventory = GameObject.FindGameObjectWithTag("inventory");
            player.inventory = inventory?.GetComponent<InventoryHolder>();

            enemyContainer = GameObject.FindGameObjectWithTag("Enemies");
            playerContainer = GameObject.FindGameObjectWithTag("Players");
            cursor = GameObject.FindGameObjectWithTag("cursor");

            RollInitiative();
        }

        public void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SubscribeToEvents()
        {
            GameEvents.current.OnLoadEvent += DestroyMe;
            GameEvents.current.OnTurnStart += (_combatController.StartTurn, GetInRangeTiles );
            GameEvents.current.OnTurnEnd += (_combatController.StopTurn, ResetTiles);
            GameEvents.current.OnInventoryUpdate += (inventory) => player.inventory = inventory;
            GameEvents.current.BroadcastMessage();
            GameEvents.current.OnEnemiesChanged += (enemies) => _enemies = enemies;
        }

        private void DestroyMe()
        {
            GameEvents.current.OnLoadEvent -= DestroyMe;
            Destroy(gameObject);
        }

        private void Update()
        {
            if (!_combatController.isTurn) return;

            GetInRangeTiles();
            HandleCursorMovement();

            if (player.hasMovement)
            {
                HandleMovement();
            }
            else if (player.hasAttack)
            {
                HandleAttack();
            }
        }

        private void HandleCursorMovement()
        {
            var hit = GetFocusedOnTile();
            if (hit.HasValue && hit.Value.collider.gameObject.CompareTag("OverlayTile"))
            {
                _overlayTile = hit.Value.collider.gameObject.GetComponent<OverlayTile>();
                cursor.transform.position = _overlayTile.transform.position;
            }
        }

        private void HandleMovement()
        {
            if (!_isMoving && _overlayTile)
            {
                _path = _pathFinder.FindPath(player.standingOnTile, _overlayTile, _rangeFinderTiles, false);
                HighlightPath();
            }

            MoveCharacter(_overlayTile);
        }

        private void HandleAttack()
        {
            var hit = GetFocusedOnTile();
            if (hit.HasValue && hit.Value.collider.gameObject.CompareTag("enemy"))
            {
                _selectedEnemy = hit.Value.collider.gameObject.GetComponent<EnemyController>();
                cursor.transform.position = new Vector3(_selectedEnemy.transform.position.x, _selectedEnemy.transform.position.y + 1, _selectedEnemy.transform.position.z);

                if (Input.GetMouseButtonDown(0) && _selectedEnemy && EnemyInWeaponRange())
                {
                    AttackEnemy();
                }
            }
        }

        private void HighlightPath()
        {
            foreach (var tile in _path)
            {
                if (tile != _overlayTile)
                    tile.SetSprite();
                tile.ShowTile();
            }
            _overlayTile.ShowTile();
        }

        private void MoveCharacter(OverlayTile tile)
        {
            if (Input.GetMouseButtonDown(0) && tile != null && !tile.isBlocked && _rangeFinderTiles.Contains(tile))
            {
                _isMoving = true;
                ResetTiles();
            }

            if (_path.Count > 0 && _isMoving)
            {
                MoveAlongPath();
            }
        }

        private void MoveAlongPath()
        {
            var step = speed * Time.deltaTime;
            var zIndex = _path[0].transform.position.z;

            player.transform.position = Vector2.MoveTowards(player.transform.position, _path[0].transform.position, step);
            player.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, zIndex);

            if (Vector2.Distance(player.transform.position, _path[0].transform.position) < 0.00001f)
            {
                PositionCharacterOnTile(_path[0]);
                _path.RemoveAt(0);
            }

            if (_path.Count == 0)
            {
                _isMoving = false;
                player.hasMovement = false;
                GetInRangeTiles();
            }
        }

        private void PositionCharacterOnTile(OverlayTile tile)
        {
            var tilePos = tile.transform.position;
            player.transform.position = new Vector3(tilePos.x, tilePos.y + 0.0001f, tilePos.z);
            player.standingOnTile = tile;
        }

        private void AttackEnemy()
        {
            _combatController.AttackOtherCharacter(player, _selectedEnemy);
            player.hasAttack = false;
        }

        private void ResetTiles()
        {
            foreach (var overlayTile in MapManager.Instance.Map.Values)
            {
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
            _rangeFinderTiles = _rangeFinder.GetTilesInRange(
                new Vector2Int(player.standingOnTile.gridLocation.x, player.standingOnTile.gridLocation.y),
                CharacterMovementRange);
        }

        private static RaycastHit2D? GetFocusedOnTile()
        {
            var mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
            var mousePos2D = new Vector2(mousePos.x, mousePos.y);

            var hits = Physics2D.RaycastAll(mousePos2D, Vector2.zero);
            return hits.Length > 0 ? hits.OrderByDescending(i => i.collider.transform.position.z).First() : null;
        }

        private bool EnemyInWeaponRange()
        {
            var distance = Vector2.Distance(player.standingOnTile.Grid2DLocation, _selectedEnemy.standingOnTile.Grid2DLocation);
            return distance <= player.weapon.weaponRange;
        }

        private void RollInitiative()
        {
            player.initiative = Random.Range(1, 20);
            Debug.Log($"Player Initiative: {player.initiative}");
        }
    }
}
