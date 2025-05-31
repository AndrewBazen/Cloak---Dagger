using System;
using UnityEngine;
using Start.Scripts.Game;

namespace Start.Scripts.BaseClasses
{
    public class Manager : MonoBehaviour
    {
        public event Action OnInitialized;
        public event Action OnDestroyed;
        private GameManager _gameManager => GameManager.Instance;

        protected virtual void Start()
        {
            Initialize();
        }

        protected virtual void Initialize()
        {
            InitializeManager();
            // Custom initialization logic can be added in derived classes
            OnInitialized?.Invoke();
        }

        protected virtual void InitializeManager()
        {
            // override this method in derived classes to perform specific initialization tasks
        }


        protected virtual void OnDestroy()
        {
            OnDestroyed?.Invoke();
        }
    }
}

