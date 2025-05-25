// SceneEventManager.cs

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Start.Scripts.Systems
{
    /// <summary>
    /// Central event dispatcher for scene-level events.
    /// Other systems can subscribe without tightly coupling to scene logic.
    /// </summary>
    public class SceneEventManager : MonoBehaviour
    {
        public static SceneEventManager Instance { get; private set; }

        public event Action<Scene, LoadSceneMode> OnSceneLoaded;
        public event Action<Scene> OnSceneUnloaded;
        public event Action<Scene, Scene> OnActiveSceneChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += HandleSceneLoaded;
            SceneManager.sceneUnloaded += HandleSceneUnloaded;
            SceneManager.activeSceneChanged += HandleActiveSceneChanged;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            OnSceneLoaded?.Invoke(scene, mode);
        }

        private void HandleSceneUnloaded(Scene scene)
        {
            OnSceneUnloaded?.Invoke(scene);
        }

        private void HandleActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            OnActiveSceneChanged?.Invoke(oldScene, newScene);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneUnloaded -= HandleSceneUnloaded;
            SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
        }
    }
}

