
// CombatManager.cs
using TMPro;
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
        public event Action OnCharacterDamaged;
        public event Action OnEnemyDamaged;

        private List<GameObject> party;
        private List<GameObject> enemies;
        private Queue<MonoBehaviour> turnQueue;
        private MonoBehaviour currentTurnActor;
        private GameObject _dmgText;
        [SerializeField] private GameObject dmgPrefab;

        public void Initialize(PartyManager partyManager, EnemyManager enemyManager)
        {
            party = new List<GameObject>(partyManager.PartyObjects);
            enemies = new List<GameObject>(enemyManager.EnemyObjects);
            turnQueue = new Queue<MonoBehaviour>();
            BuildTurnOrder();
        }

        private void BuildTurnOrder()
        {
            turnQueue.Clear();
            List<GameObject> all = new();
            all.AddRange(party);
            all.AddRange(enemies);

            all.Sort((a, b) =>
            {
                int aInit = GetInitiative(a);
                int bInit = GetInitiative(b);
                return bInit.CompareTo(aInit);
            });

            foreach (var actor in all)
                turnQueue.Enqueue(actor.GetComponent<MonoBehaviour>());

            StartNextTurn();
            OnCombatStarted?.Invoke();
        }

        private int GetInitiative(GameObject actor)
        {
            if (actor.GetComponent<PlayerController>())
            {
                return actor.GetComponent<PlayerController>().Initiative;
            }
            return actor.GetComponent<EnemyController>().initiative;
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

        public void DamageCharacter(PlayerController player, int damage)
        {
            if (player == null) return;
            _dmgText = Instantiate(dmgPrefab, player.gameObject.transform);
            _dmgText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(damage.ToString());
            OnCharacterDamaged?.Invoke();
        }

        public void DamageEnemy(EnemyController enemy, int damage)
        {
            if (enemy == null) return;
            _dmgText = Instantiate(dmgPrefab, enemy.gameObject.transform);
            _dmgText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(damage.ToString());
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

        public int RollInitiative(CharacterInfoData characterData)
        {
            // Example initiative roll, can be replaced with actual game logic
            return UnityEngine.Random.Range(1, 21);
        }
    }
}

