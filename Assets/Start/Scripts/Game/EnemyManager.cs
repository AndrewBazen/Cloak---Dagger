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
        public static EnemyManager Instance { get; private set; }
        public event Action<List<EnemyController>> OnEnemiesChanged;
        public event Action<EnemyController> OnEnemyDied;

        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject enemyContainer;

        public List<EnemyController> CurrentEnemies => _enemies;
        public List<GameObject> EnemyObjects => _enemyObjects;

        private List<GameObject> _enemyObjects;
        private List<EnemyController> _enemies;

        private float difficultyMultiplier;
        private GameManager _gameManager;
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        public void Initialize()
        {
            _gameManager = GameManager.Instance;
            _enemies = new List<EnemyController>();
            _enemyObjects = new List<GameObject>();
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
            OnEnemiesChanged?.Invoke(_enemies);
        }

        private EnemyController SpawnEnemy(Vector3 position)
        {
            var enemyObj = Instantiate(enemyPrefab, position, Quaternion.identity, enemyContainer.transform);
            var controller = enemyObj.GetComponent<EnemyController>();

            _enemyObjects.Add(enemyObj);
            _enemies.Add(controller);
            return controller;
        }

        public void SpawnEnemies(List<EnemyData> enemies)
        {
            foreach (var enemy in enemies)
            {
                SpawnEnemy(enemy.Position);
            }
        }

        private void ApplyDifficulty(EnemyController enemy)
        {
            enemy.SetDifficulty(difficultyMultiplier);
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
            if (!_enemies.Contains(enemy)) return;
            _enemies.Remove(enemy);
            _enemyObjects.Remove(enemy.gameObject);
            Destroy(enemy.gameObject);

            OnEnemiesChanged?.Invoke(_enemies);
            OnEnemyDied?.Invoke(enemy);
        }

        private void ClearEnemies()
        {
            foreach (var obj in _enemyObjects)
            {
                if (obj != null)
                    Destroy(obj);
            }
            _enemies.Clear();
            _enemyObjects.Clear();
        }

        public void SetDifficultyMultiplier(float multiplier)
        {
            difficultyMultiplier = multiplier;
        }
    }
}

