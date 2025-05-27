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
        public PartyManager Party => partyManager;
        public EnemyManager Enemies => enemyManager;
        public CombatManager Combat => combatManager;
        public MapManager MapMan => mapManager;
        public UIManager UI => uiManager;
        public SceneEventManager SceneEvents => sceneEventManager;
        public InputManager Input => inputManager;
        public JobSystemPathFinder PathFinder => JobSystemPathFinder.Instance;
        public JobSystemRangeFinder RangeFinder => JobSystemRangeFinder.Instance;

        public enum GameState { MainMenu, Initialization, Exploration, Combat, Paused, GameOver }
        private GameState _currentGameState;

        public GameState Current => _currentGameState;
        public event System.Action OnMapGenerated;

        [Header("Managers")]
        [SerializeField] private PartyManager partyManager;
        [SerializeField] private EnemyManager enemyManager;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject playerContainer;
        [SerializeField] private MapManager mapManager;
        [SerializeField] private CombatManager combatManager;
        [SerializeField] private InputManager inputManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private SceneEventManager sceneEventManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            sceneEventManager.OnSceneLoaded += HandleSceneLoaded;
            inputManager.OnPausePressed += TogglePause;

            _currentGameState = GameState.MainMenu;
            uiManager.ShowMainMenu();
        }

        public void StartNewGame()
        {
            _currentGameState = GameState.Exploration;

            partyManager.Initialize();
            enemyManager.Initialize();
            combatManager.Initialize();

            combatManager.OnCombatStarted += () =>
            {
                _currentGameState = GameState.Combat;
                uiManager.ShowCombatUI();
            };

            combatManager.OnCombatEnded += () =>
            {
                _currentGameState = GameState.Exploration;
                uiManager.ShowExplorationUI();
            };

            LoadLevel(1);
            uiManager.ShowGameplayUI();
        }

        public void TogglePause()
        {
            if (_currentGameState == GameState.Paused)
            {
                _currentGameState = GameState.Exploration;
                Time.timeScale = 1.0f;
                uiManager.HidePauseMenu();
            }
            else
            {
                _currentGameState = GameState.Paused;
                Time.timeScale = 0.0f;
                uiManager.ShowPauseMenu();
            }
        }

        public void EndGame()
        {
            _currentGameState = GameState.GameOver;
            uiManager.ShowGameOver();
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

            var spawnedParty = partyManager.SpawnPlayers(
                partyData,
                map.playerSpawnTiles,
                playerPrefab,
                playerContainer.transform
            );


            OnMapGenerated?.Invoke();
            enemyManager.SpawnEnemiesForLevel(1);
            uiManager.ShowGameplayUI();
        }

    }
}
