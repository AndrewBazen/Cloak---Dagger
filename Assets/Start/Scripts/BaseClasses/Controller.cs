using System;
using UnityEngine;
using Start.Scripts.Game;
using System.Linq;
using System.Collections.Generic;
using Start.Scripts.Character;
using Start.Scripts.Enemy;

namespace Start.Scripts.BaseClasses
{
    public class Controller : MonoBehaviour
    {
        public event Action OnInitialized;
        public event Action OnDestroyed;
        public GameManager _gameManager => GameManager.Instance;
        protected Controller _controller;
        private IEnumerable<Controller> _controllersInParent;

        protected virtual void Start()
        {
            InitializeController();
        }

        protected void InitializeController()
        {
            GetControllerInParent();
            // Custom initialization logic can be added in derived classes
            OnInitialized?.Invoke();
        }

        protected void GetControllerInParent()
        {
            var controllers = GetComponentsInParent<Controller>();
            _controllersInParent = controllers.Where(x => x != this);
            _controller = _controllersInParent.FirstOrDefault(c => c.GetType() == typeof(PlayerController) || c.GetType() == typeof(EnemyController));
        }

        protected void OnDestroy()
        {
            OnDestroyed?.Invoke();
        }
    }
}
