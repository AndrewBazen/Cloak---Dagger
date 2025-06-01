// GameManager.cs (Updated for Modular Architecture)
using UnityEngine;
using UnityEngine.SceneManagement;
using Start.Scripts.Map;
using Start.Scripts.UI;
using Start.Scripts.AI;
using System.Collections.Generic;
using Start.Scripts.Classes;
using Start.Scripts.Character;

namespace Start.Scripts.Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public PartyManager Party => _partyManager;
        public EnemyManager Enemies => _enemyManager;
        public CombatManager Combat => _combatManager;
        public MapManager MapMan => _mapManager;
        public UIManager UI => _uiManager;
        public SceneEventManager SceneEvents => _sceneEventManager;
        public InputManager Input => _inputManager;
        public JobSystemPathFinder PathFinder => JobSystemPathFinder.Instance;
        public JobSystemRangeFinder RangeFinder => JobSystemRangeFinder.Instance;
        public GameObject DamageTextPrefab => _dmgPrefab;
        public GameObject Cursor => _cursor;
        public enum GameState { MainMenu, Initialization, Exploration, Combat, Paused, GameOver }
        private GameState _currentGameState;

        private GameState _previousGameState;

        public GameState CurrentGameState => _currentGameState;
        private SaveManager _saveManager;
        public SaveManager SaveManager => _saveManager;
        public event System.Action OnMapGenerated;
        public event System.Action OnGameStateChanged;

        [Header("Managers")]
        [SerializeField] private GameObject _dmgPrefab;
        [SerializeField] private PartyManager _partyManager;
        [SerializeField] private EnemyManager _enemyManager;
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private GameObject _playerContainer;
        [SerializeField] private MapManager _mapManager;
        [SerializeField] private CombatManager _combatManager;
        [SerializeField] private InputManager _inputManager;
        [SerializeField] private UIManager _uiManager;
        [SerializeField] private SceneEventManager _sceneEventManager;
        [SerializeField] private GameObject _cursor;




        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            InitializeManagers();
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            _sceneEventManager.OnSceneLoaded += HandleSceneLoaded;
            _inputManager.OnPausePressed += TogglePause;
            //_combatManager.OnPlayerDefeated += HandlePlayerDefeated;
            //_combatManager.OnEnemyDefeated += HandleEnemyDefeated;


            _currentGameState = GameState.MainMenu;
            _previousGameState = GameState.MainMenu;
            _uiManager.ShowMainMenu();
        }

        private void InitializeManagers()
        {
            _partyManager = PartyManager.Instance;
            _enemyManager = EnemyManager.Instance;
            _combatManager = CombatManager.Instance;
            _mapManager = MapManager.Instance;
            _uiManager = UIManager.Instance;
            _sceneEventManager = SceneEventManager.Instance;
            _inputManager = InputManager.Instance;

            // Subscribe to game state changes
            OnGameStateChanged += () => Debug.Log($"Game state changed to: {_currentGameState}");
        }

        public void Initialize()
        {
            _currentGameState = GameState.MainMenu;
            _uiManager.ShowMainMenu();
        }

        public void LoadGame(string saveName)
        {
            _currentGameState = GameState.Exploration;
            _uiManager.ShowGameplayUI();
        }

        public void StartNewGame()
        {
            _currentGameState = GameState.Exploration;

            _partyManager.Initialize();
            _enemyManager.Initialize();
            _combatManager.Initialize();

            _combatManager.OnCombatStarted += () =>
            {
                _currentGameState = GameState.Combat;
                _uiManager.ShowCombatUI();
            };

            _combatManager.OnCombatEnded += () =>
            {
                _currentGameState = GameState.Exploration;
                _uiManager.ShowExplorationUI();
            };

            LoadLevel(1);
            _uiManager.ShowGameplayUI();
        }

        public void TogglePause()
        {
            if (_currentGameState == GameState.Paused && _previousGameState == GameState.Exploration)
            {
                _currentGameState = GameState.Exploration;
                Time.timeScale = 1.0f;
                _uiManager.HidePauseMenu();
            }
            else if (_currentGameState == GameState.Paused && _previousGameState == GameState.Combat)
            {
                _currentGameState = GameState.Combat;
                Time.timeScale = 1.0f;
                _uiManager.HidePauseMenu();
            }
            else if (_currentGameState != GameState.Paused)
            {
                _previousGameState = _currentGameState;
                _currentGameState = GameState.Paused;
                Time.timeScale = 0.0f;
                _uiManager.ShowPauseMenu();
            }
            else
            {
                return;
            }
        }

        public void EndGame()
        {
            _previousGameState = _currentGameState;
            _currentGameState = GameState.GameOver;
            _uiManager.ShowGameOver();
            SceneManager.LoadScene("GameOver");
        }

        private void LoadLevel(int level)
        {
            _previousGameState = _currentGameState;
            _currentGameState = GameState.Exploration;
            string sceneName = $"Level_{level}";
            SceneManager.LoadScene(sceneName);
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name.Contains("Level_"))
            {
                if (MapMan == null || MapMan.Map == null || MapMan.playerSpawnTiles == null)
                {
                    Debug.LogError("Map not found in scene");
                return;
                }

                // Example character data
                var partyData = new List<CharacterInfoData>
                {
                    new CharacterInfoData { Id = 1, PlayerClass = new PlayerClass { className = "Warrior" } },
                    new CharacterInfoData { Id = 2, PlayerClass = new PlayerClass { className = "Rogue" } },
                };

                var spawnedParty = _partyManager.SpawnPlayers(
                    partyData,
                    MapMan.playerSpawnTiles
                );


                OnMapGenerated?.Invoke();
                _enemyManager.SpawnEnemiesForLevel(1);
                _uiManager.ShowGameplayUI();
            }
        }

        public void Confirm()
        {
            // Future: Send confirm event to currently focused UI or system
            Debug.Log("Confirm Input Received");
        }

        public void Cancel()
        {
            // Future: Close menus, cancel actions
            Debug.Log("Cancel Input Received");
        }

    }
}
