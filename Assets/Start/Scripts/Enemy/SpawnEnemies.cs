using UnityEngine;

namespace Start.Scripts.Enemy
{
    public class SpawnEnemies : MonoBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject enemyContainer;
        private EnemyController _enemyController;

        // Update is called once per frame
        void Update()
        {
            if (!_enemyController)
            {
                foreach (var tile in MapManager.Instance.enemySpawnTiles)
                {
                    SpawnEnemy();
                    PositionEnemyOnLine(tile);
                }
            }
        }
        
        private void SpawnEnemy()
        {
            _enemyController = Instantiate(enemyPrefab, enemyContainer.transform).GetComponent<EnemyController>();
        }
        
        private void PositionEnemyOnLine(OverlayTile tile)
        {
            var tilePos = tile.transform.position;
            _enemyController.transform.position = new Vector3(tilePos.x, tilePos.y + 0.0001f, tilePos.z);
            _enemyController.standingOnTile = tile; 
        }

    }
}
