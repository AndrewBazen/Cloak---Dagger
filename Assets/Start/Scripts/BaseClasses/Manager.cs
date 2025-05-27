using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Start.Scripts.Game;
using Start.Scripts.Character;
using Start.Scripts.Enemy;

namespace Start.Scripts.BaseClasses
{
    public class Manager : MonoBehaviour
    {
        public event Action OnInitialized;
        public event Action OnDestroyed;
        private GameManager _gameManager;
        private Manager _managerInParent;
        private List<Manager> _managers;
        private List<GameObject> _managerObjects;

        protected virtual void Start()
        {
            Initialize();
        }

        protected virtual void Initialize()
        {
            _gameManager = GameManager.Instance;
            // Custom initialization logic can be added in derived classes
            OnInitialized?.Invoke();
        }


        protected virtual void OnDestroy()
        {
            OnDestroyed?.Invoke();
        }
    }
}

