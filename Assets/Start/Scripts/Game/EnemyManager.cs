// EnemyManager.cs

using System;
using System.Collections.Generic;
using Start.Scripts.Enemy;
using UnityEngine;

namespace Start.Scripts.Game
{
    public class EnemyManager : MonoBehaviour
    {
        public event Action<List<EnemyController>> OnEnemiesChanged;
        public event Action<EnemyController> OnEnemyDied;

        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject enemyContainer;

        private List<GameObject> enemyObjects = new();
        private List<EnemyController> enemies = new();
        private float difficultyMultiplier = 1.0f;

        public IReadOnlyList<EnemyController> CurrentEnemies => enemies;

        public void Initialize()
        {
            enemies.Clear();
            enemyObjects.Clear();
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
            return new Vector3(UnityEngine.Random.Range(-5, 5), UnityEngine.Random.Range(-5, 5), 0);
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

