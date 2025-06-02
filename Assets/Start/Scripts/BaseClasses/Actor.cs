using System;
using Start.Scripts.Interfaces;
using Start.Scripts.Game;
using Start.Scripts.Combat;
using System.ComponentModel;
using Start.Resources;
using Random = System.Random;
using Component = UnityEngine.Component;
using System.Collections.Generic;
using UnityEngine;

namespace Start.Scripts.BaseClasses
{
    public abstract class Actor : MonoBehaviour, ITrackable, INotifyPropertyChanged
    {

        protected bool _isTurn;
        protected bool _hasAction;
        protected bool _hasBonusAction;
        protected bool _hasReaction;
        protected int _initiative;
        protected OverlayTile _standingOnTile;
        protected int _currentHealth;
        protected int _currentMana;
        protected bool _hasMovement;
        protected bool _isMoving;
        protected List<OverlayTile> _path;
        protected Dictionary<string, int> _statBonuses;
        protected List<Actor> _controllersInParent;
        protected Actor _controller;
        protected CombatController _combatController;

        public event Action OnTurnStarted;
        public event Action OnTurnEnded;
        public event PropertyChangedEventHandler PropertyChanged;
        public event Action OnInitialized;
        public event Action OnDestroyed;

        #region Properties
        public GameManager _gameManager => GameManager.Instance;
        public bool HasTurn
        {
            get => _isTurn;
            set
            {
                _isTurn = value;
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
            if (gameObject.GetComponent<CombatController>() == null)
            {
                // initialize combat controller
                _combatController = gameObject.AddComponent<CombatController>();
            }
            // Custom initialization logic for actors can be added here
        }

        public virtual void StartTurn()
        {
            _isTurn = true;
            OnTurnStarted?.Invoke();
        }

        public virtual void EndTurn()
        {
            _isTurn = false;
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
        protected void RollInitiative()
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
