using System;
using Start.Scripts.Interfaces;
using Start.Scripts.Enemy;
using Start.Scripts.Game;
using Start.Scripts.Combat;
using Start.Scripts.Character;
using System.ComponentModel;
using Start.Resources;
using Random = System.Random;
using Component = UnityEngine.Component;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Start.Scripts.BaseClasses
{
    public abstract class Actor : MonoBehaviour, ITrackable, INotifyPropertyChanged
    {

        private bool _hasTurn;
        private bool _hasAction;
        private bool _hasBonusAction;
        private bool _hasReaction;
        private int _initiative;
        private OverlayTile _standingOnTile;
        private int _currentHealth;
        private int _currentMana;
        private bool _hasMovement;
        private bool _isMoving;
        private List<OverlayTile> _path = new List<OverlayTile>();
        private Dictionary<string, int> _statBonuses;
        private List<Actor> _controllersInParent;
        private Actor _controller;
        private Component _combatController;

        public event Action OnTurnStarted;
        public event Action OnTurnEnded;
        public event PropertyChangedEventHandler PropertyChanged;
        public event Action OnInitialized;
        public event Action OnDestroyed;

        #region Properties
        public GameManager _gameManager => GameManager.Instance;
        public bool HasTurn
        {
            get => _hasTurn;
            set
            {
                _hasTurn = value;
                OnPropertyChanged(nameof(HasTurn));
            }
        }
        public bool HasAction
        {
            get => _hasAction;
            set
            {
                _hasAction = value;
                OnPropertyChanged(nameof(HasAction));
            }
        }
        public bool HasBonusAction
        {
            get => _hasBonusAction;
            set
            {
                _hasBonusAction = value;
                OnPropertyChanged(nameof(HasBonusAction));
            }
        }
        public bool HasReaction
        {
            get => _hasReaction;
            set
            {
                _hasReaction = value;
                OnPropertyChanged(nameof(HasReaction));
            }
        }
        public int Initiative
        {
            get => _initiative;
            set
            {
                _initiative = value;
                OnPropertyChanged(nameof(Initiative));
            }
        }
        public OverlayTile StandingOnTile
        {
            get => _standingOnTile;
            set
            {
                _standingOnTile = value;
                OnPropertyChanged(nameof(StandingOnTile));
            }
        }
        public int CurrentHealth
        {
            get => _currentHealth;
            set
            {
                _currentHealth = value;
                OnPropertyChanged(nameof(CurrentHealth));
            }
        }
        public int CurrentMana
        {
            get => _currentMana;
            set
            {
                _currentMana = value;
                OnPropertyChanged(nameof(CurrentMana));
            }
        }
        public bool HasMovement
        {
            get => _hasMovement;
            set
            {
                _hasMovement = value;
                OnPropertyChanged(nameof(HasMovement));
            }
        }

        public Dictionary<string, int> StatBonuses
        {
            get => _statBonuses;
            set
            {
                _statBonuses = value;
                OnPropertyChanged(nameof(StatBonuses));
            }
        }
        #endregion

        protected virtual void OnEnable()
        {
            ActorRegistry.Register(this);
        }

        protected virtual void OnDisable()
        {
            ActorRegistry.Unregister(this);
            OnDestroyed?.Invoke();
        }

        protected virtual void Start()
        {
            InitializeActor();
            _gameManager.Combat.OnCombatStarted += OnCombatStarted;
            _gameManager.Combat.OnCombatEnded += OnCombatEnded;
            OnInitialized?.Invoke();
        }

        protected virtual void InitializeActor()
        {
            if (_combatController == null)
            {
                // initialize combat controller
                _combatController = gameObject.GetComponent<CombatController>();
            }
            // Custom initialization logic for actors can be added here
        }

        public virtual void StartTurn()
        {
            HasTurn = true;
            OnTurnStarted?.Invoke();
        }

        public virtual void EndTurn()
        {
            HasTurn = false;
            OnTurnEnded?.Invoke();
        }

        protected virtual void OnCombatStarted()
        {
            // Reset for combat
            _hasMovement = true;
            _hasAction = true;
            _hasBonusAction = true;
            _hasReaction = true;
            _path.Clear();
            _isMoving = false;
            RollInitiative();
        }

        protected virtual void OnCombatEnded()
        {
            // Reset after combat
            _path.Clear();
            _isMoving = false;
            _hasMovement = false;
            _hasAction = false;
            _hasBonusAction = false;
            _hasReaction = false;
            _initiative = 0;
        }

        /** RollInitiative()
         * description: gets the initiative of the current enemy.
         * @return void
         */
        private void RollInitiative()
        {
            var rand = new Random();
            Initiative = rand.Next(1, 20);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
