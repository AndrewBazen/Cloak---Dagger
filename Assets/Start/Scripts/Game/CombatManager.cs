
// CombatManager.cs
using TMPro;
using System;
using System.Collections.Generic;
using Start.Scripts.Character;
using Start.Scripts.Enemy;
using Start.Scripts.Combat;
using Start.Scripts.BaseClasses;
using UnityEngine;

namespace Start.Scripts.Game
{
    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance { get; private set; }
        public event Action OnCombatStarted;
        public event Action OnCombatEnded;
        public event Action OnCharacterDamaged;
        public event Action OnEnemyDamaged;

        private List<Actor> _party;
        private List<Actor> _enemies;
        private Queue<Actor> _turnQueue;
        private MonoBehaviour _currentTurnActor;
        private GameManager _gameManager;


        private void Awake()
        {

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Initialize();
            DontDestroyOnLoad(gameObject);
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager instance is null. CombatManager cannot function without it.");
                return;
            }
            _gameManager = GameManager.Instance;
        }

        public void Initialize()
        {
            _party = _gameManager.Party.Actors;
            _enemies = _gameManager.Enemies.Actors;
            _turnQueue = new Queue<Actor>();
            BuildTurnOrder();
            if (_turnQueue.Count == 0)
            {
                Debug.LogWarning("No actors in turn queue. Combat cannot start.");
                return;
            }
        }

        private void BuildTurnOrder()
        {
            _turnQueue.Clear();
            List<Actor> all = new();
            all.AddRange(_party);
            all.AddRange(_enemies);

            all.Sort((a, b) =>
            {
                int aInit = GetInitiative(a);
                int bInit = GetInitiative(b);
                return bInit.CompareTo(aInit);
            });

            foreach (var actor in all)
                _turnQueue.Enqueue(actor);

            StartNextTurn();
            OnCombatStarted?.Invoke();
        }

        private int GetInitiative(Actor actor)
        {
            if (actor.GetType() == typeof(PlayerController))
            {
                return actor.GetComponent<PlayerController>().Initiative;
            }
            return actor.GetComponent<EnemyController>().Initiative;
        }

        public void EndTurn()
        {
            if (_currentTurnActor == null)
            {
                Debug.LogWarning("Current turn actor is null. Cannot end turn.");
                return;
            }
            _currentTurnActor.GetComponent<CombatController>().StopTurn();

            StartNextTurn();
        }

        // updated to use the Actor class which is agnostic of type of actor
        public void DamageActor(Actor actor, int damage)
        {
            if (actor == null) return;
            var dmgText = Instantiate(_gameManager.DamageTextPrefab, actor.gameObject.transform);
            dmgText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(damage.ToString());
            OnCharacterDamaged?.Invoke();
        }

        private void StartNextTurn()
        {
            if (_turnQueue.Count == 0)
            {
                EndCombat();
                return;
            }

            _currentTurnActor = _turnQueue.Dequeue();
            if (_currentTurnActor == null)
            {
                Debug.LogWarning("Current turn actor is null. Skipping turn.");
                StartNextTurn();
                return;
            }
            _currentTurnActor.GetComponent<CombatController>().StartTurn();
        }

        private void StartCombat()
        {
            if (_party.Count == 0 || _enemies.Count == 0)
            {
                Debug.LogWarning("Cannot start combat without party or enemies.");
                return;
            }
            BuildTurnOrder();
            OnCombatStarted?.Invoke();
        }

        private void EndCombat()
        {
            OnCombatEnded?.Invoke();
        }
    }
}

