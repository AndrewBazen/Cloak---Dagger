// UIManager.cs

using UnityEngine;
using UnityEngine.SceneManagement;
using Start.Scripts.Game;

namespace Start.Scripts.UI
{
    /// <summary>
    /// Manages UI elements based on scene and game state.
    /// Listens to scene events and updates UI accordingly.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject gameOverPanel;

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
            if (GameManager.Instance.SceneEvents != null)
            {
                GameManager.Instance.SceneEvents.OnSceneLoaded += HandleSceneLoaded;
                GameManager.Instance.SceneEvents.OnActiveSceneChanged += HandleActiveSceneChanged;
            }
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
        }

        public void TogglePauseMenu(bool show)
        {
            pauseMenuPanel?.SetActive(show);
        }

        private void OnDestroy()
        {
            if (SceneEventManager.Instance != null)
            {
                SceneEventManager.Instance.OnSceneLoaded -= HandleSceneLoaded;
                SceneEventManager.Instance.OnActiveSceneChanged -= HandleActiveSceneChanged;
            }
        }
    }
}
