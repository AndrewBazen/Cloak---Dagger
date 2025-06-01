// UIManager.cs

using UnityEngine;
using UnityEngine.SceneManagement;
using Start.Scripts.Game;
using Start.Scripts.Character;
using System.Collections.Generic;


namespace Start.Scripts.UI
{
    /// <summary>
    /// Manages UI elements based on scene and game state.
    /// Listens to scene events and updates UI accordingly.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        private GameManager _gameManager;

        [Header("UI Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject pauseMenuPanel;

        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject gamePlayPanel;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            _gameManager = GameManager.Instance;
        }

        private void Start()
        {
            Initialize();
            if (_gameManager.SceneEvents != null)
            {
                _gameManager.SceneEvents.OnSceneLoaded += HandleSceneLoaded;
                _gameManager.SceneEvents.OnActiveSceneChanged += HandleActiveSceneChanged;
            }
        }

        private void Initialize()
        {
            _gameManager.OnGameStateChanged += UpdateUI;
            _gameManager.Party.OnPartyUpdated += UpdatePartyUI;
            _gameManager.Combat.OnCombatStarted += UpdateCombatUI;
            _gameManager.Combat.OnCombatEnded += UpdateUI;
            UpdateUIForScene("MainMenu");
        }

        private void UpdateUI()
        {
            if (_gameManager.CurrentGameState == GameManager.GameState.Exploration)
            {
                UpdatePartyUI(_gameManager.Party.PartyMembers);
            }
            else if (_gameManager.CurrentGameState == GameManager.GameState.Combat)
            {
                UpdateCombatUI();
            }
        }

        private void UpdatePartyUI(List<CharacterInfoData> party)
        {
            // TODO: Update party portrait panel, turn order UI, etc.
        }

        private void UpdateCombatUI()
        {
            // TODO: Update combat UI
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            UpdateUIForScene(scene.name);
        }

        private void HandleActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            UpdateUIForScene(newScene.name);
        }

        public void ShowMainMenu()
        {
            UpdateUIForScene("MainMenu");
        }

        public void ShowCombatUI()
        {
            UpdateUIForScene("Combat");
        }
        public void ShowGameOver()
        {
            UpdateUIForScene("GameOver");
        }

        public void ShowExplorationUI()
        {
            UpdateUIForScene("Exploration");
        }

        public void ShowGameplayUI()
        {
            UpdateUIForScene("GamePlay");
        }

        public void HidePauseMenu()
        {
            TogglePauseMenu(false);
        }

        public void ShowPauseMenu()
        {
            TogglePauseMenu(true);
        }

        private void UpdateUIForScene(string sceneName)
        {
            mainMenuPanel?.SetActive(sceneName == "MainMenu");
            hudPanel?.SetActive(sceneName.StartsWith("Level_"));
            pauseMenuPanel?.SetActive(false);
            gameOverPanel?.SetActive(sceneName == "GameOver");
            gamePlayPanel?.SetActive(sceneName == "GamePlay");
        }

        public void TogglePauseMenu(bool show)
        {
            pauseMenuPanel?.SetActive(show);
        }

        private void OnDestroy()
        {
            if (_gameManager.SceneEvents != null)
            {
                _gameManager.SceneEvents.OnSceneLoaded -= HandleSceneLoaded;
                _gameManager.SceneEvents.OnActiveSceneChanged -= HandleActiveSceneChanged;
            }
        }
    }
}
