
// CombatManager.cs

using System;
using System.Collections.Generic;
using Start.Scripts.Character;
using Start.Scripts.Enemy;
using UnityEngine;

namespace Start.Scripts.Game
{
    public class CombatManager : MonoBehaviour
    {
        public event Action OnCombatStarted;
        public event Action OnCombatEnded;

        private List<CharacterInfo> party;
        private List<EnemyController> enemies;
        private Queue<MonoBehaviour> turnQueue;
        private MonoBehaviour currentTurnActor;

        public void Initialize(PartyManager partyManager, EnemyManager enemyManager)
        {
            party = new List<CharacterInfo>(partyManager.CurrentParty);
            enemies = new List<EnemyController>(enemyManager.CurrentEnemies);
            turnQueue = new Queue<MonoBehaviour>();
            BuildTurnOrder();
        }

        private void BuildTurnOrder()
        {
            turnQueue.Clear();
            List<MonoBehaviour> all = new();
            all.AddRange(party);
            all.AddRange(enemies);

            all.Sort((a, b) =>
            {
                int aInit = GetInitiative(a);
                int bInit = GetInitiative(b);
                return bInit.CompareTo(aInit);
            });

            foreach (var actor in all)
                turnQueue.Enqueue(actor);

            StartNextTurn();
            OnCombatStarted?.Invoke();
        }

        private int GetInitiative(MonoBehaviour actor)
        {
            return actor switch
            {
                CharacterInfo c => c.initiative,
                EnemyController e => e.initiative,
                _ => 0
            };
        }

        public void EndTurn()
        {
            if (turnQueue.Count == 0)
            {
                EndCombat();
                return;
            }

            StartNextTurn();
        }

        private void StartNextTurn()
        {
            if (turnQueue.Count == 0)
            {
                EndCombat();
                return;
            }

            currentTurnActor = turnQueue.Dequeue();
            // Activate character or enemy turn behavior here
        }

        private void EndCombat()
        {
            OnCombatEnded?.Invoke();
        }
    }
}

