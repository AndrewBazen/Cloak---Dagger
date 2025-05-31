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

        public enum GameState { MainMenu, Initialization, Exploration, Combat, Paused, GameOver }
        private GameState _currentGameState;

        public GameState Current => _currentGameState;
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

            _currentGameState = GameState.MainMenu;
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
            OnGameStateChanged += (state) => Debug.Log($"Game state changed to: {state}");
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
            if (_currentGameState == GameState.Paused)
            {
                _currentGameState = GameState.Exploration;
                Time.timeScale = 1.0f;
                _uiManager.HidePauseMenu();
            }
            else
            {
                _currentGameState = GameState.Paused;
                Time.timeScale = 0.0f;
                _uiManager.ShowPauseMenu();
            }
        }

        public void EndGame()
        {
            _currentGameState = GameState.GameOver;
            _uiManager.ShowGameOver();
            SceneManager.LoadScene("GameOver");
        }

        private void LoadLevel(int level)
        {
            string sceneName = $"Level_{level}";
            SceneManager.LoadScene(sceneName);
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var map = MapManager.Instance;
            if (map == null || map.playerSpawnTiles == null) return;

            // Example character data
            var partyData = new List<CharacterLoadData>
            {
                new CharacterLoadData { characterName = "Aelric", playerClass = new PlayerClass { name = "Warrior" } },
                new CharacterLoadData { characterName = "Thorne", playerClass = new PlayerClass { name = "Rogue" } },
            };

            var spawnedParty = _partyManager.SpawnPlayers(
                partyData,
                map.playerSpawnTiles,
                _playerPrefab,
                _playerContainer.transform
            );


            OnMapGenerated?.Invoke();
            _enemyManager.SpawnEnemiesForLevel(1);
            _uiManager.ShowGameplayUI();
        }

    }
}
