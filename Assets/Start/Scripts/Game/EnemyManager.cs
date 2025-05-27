// EnemyManager.cs

using Start.Scripts.Map;
using System;
using System.Collections.Generic;
using Start.Scripts.Enemy;
using Start.Scripts.Character;
using UnityEngine;

namespace Start.Scripts.Game
{
    public class EnemyManager : MonoBehaviour
    {
        public event Action<List<EnemyController>> OnEnemiesChanged;
        public event Action<EnemyController> OnEnemyDied;

        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject enemyContainer;

        public List<GameObject> enemyObjects = new();
        public List<EnemyController> enemies = new();
        private float difficultyMultiplier = 1.0f;

        public IReadOnlyList<EnemyController> CurrentEnemies => enemies;
        public IReadOnlyList<GameObject> EnemyObjects => enemyObjects;
        private IReadOnlyList<CharacterInfoData> partyData;
        public void Initialize()
        {
            enemies.Clear();
            enemyObjects.Clear();
            partyData = GameManager.Instance.Party.PartyMembers;
        }

        public void SpawnEnemiesForLevel(int level)
        {
            ClearEnemies();
            int count = 2 + level;
            for (int i = 0; i < count; i++)
            {
                Vector3 position = FindSpawnPosition();
                EnemyController newEnemy = SpawnEnemy(position);
                ApplyDifficulty(newEnemy);
            }
            OnEnemiesChanged?.Invoke(enemies);
        }

        private EnemyController SpawnEnemy(Vector3 position)
        {
            var enemyObj = Instantiate(enemyPrefab, position, Quaternion.identity, enemyContainer.transform);
            var controller = enemyObj.GetComponent<EnemyController>();

            enemyObjects.Add(enemyObj);
            enemies.Add(controller);
            return controller;
        }

        private void ApplyDifficulty(EnemyController enemy)
        {
            enemy.maxHealth = Mathf.RoundToInt(enemy.maxHealth * difficultyMultiplier);
            enemy.health = enemy.maxHealth;
            enemy.bonusToHit = Mathf.RoundToInt(enemy.bonusToHit * difficultyMultiplier);
        }

        private Vector3 FindSpawnPosition()
        {
            foreach (OverlayTile spawntile in MapManager.Instance.enemySpawnTiles)
            {
                if (spawntile != null && !spawntile.isBlocked)
                {
                    return spawntile.transform.position;
                }
                var randomTile = MapManager.Instance.enemySpawnTiles[UnityEngine.Random.Range(0, MapManager.Instance.enemySpawnTiles.Count)];
                if (randomTile != null && !randomTile.isBlocked)
                {
                    return randomTile.transform.position;
                }
            }
            Debug.LogWarning("No valid spawn tile found for enemy.");
            return new Vector3(0, 0, 0); // Fallback position if no valid spawn tile is found
        }

        public void RemoveEnemy(EnemyController enemy)
        {
            if (!enemies.Contains(enemy)) return;
            enemies.Remove(enemy);
            enemyObjects.Remove(enemy.gameObject);
            Destroy(enemy.gameObject);

            OnEnemiesChanged?.Invoke(enemies);
            OnEnemyDied?.Invoke(enemy);
        }

        private void ClearEnemies()
        {
            foreach (var obj in enemyObjects)
            {
                if (obj != null)
                    Destroy(obj);
            }
            enemies.Clear();
            enemyObjects.Clear();
        }

        public void SetDifficultyMultiplier(float multiplier)
        {
            difficultyMultiplier = multiplier;
        }
    }
}

