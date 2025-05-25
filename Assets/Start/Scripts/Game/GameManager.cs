// GameManager.cs (Updated for Modular Architecture)

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Start.Scripts.Character;
using Start.Scripts.Combat;
using Start.Scripts.Enemy;
using Start.Scripts.Map;
using Start.Scripts.Scene;
using Start.Scripts.UI;

namespace Start.Scripts.Game
{
  public class GameManager : MonoBehaviour
  {
    public static GameManager Instance { get; private set; }

    public enum GameState { MainMenu, Initialization, Exploration, Combat, Paused, GameOver }
    private GameState _currentGameState;
    public GameState Current => _currentGameState;

    [Header("Managers")]
    [SerializeField] private PartyManager partyManager;
    [SerializeField] private EnemyManager enemyManager;
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
        combatManager.Initialize(partyManager, enemyManager);

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

    private void HandleSceneLoaded()
    {
      var map = MapManager.Instance;
      if (map == null || map.playerSpawnTiles == null) return;

      // Example character data
      var partyData = new List<CharacterLoadData>
      {
          new CharacterLoadData { characterName = "Aelric", playerClass = someClass },
          new CharacterLoadData { characterName = "Thorne", playerClass = anotherClass }
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

    public PartyManager Party => partyManager;
    public EnemyManager Enemies => enemyManager;
    public CombatManager Combat => combatManager;
  }
}
