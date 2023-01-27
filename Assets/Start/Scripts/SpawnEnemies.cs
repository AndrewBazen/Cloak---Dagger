using UnityEngine;

namespace Start.Scripts
{
    public class SpawnEnemies : MonoBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject enemyContainer;
        private CharacterInfo _characterInfo;

        // Update is called once per frame
        void Update()
        {
            if (_characterInfo == null)
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
            _characterInfo = Instantiate(enemyPrefab, enemyContainer.transform).GetComponent<CharacterInfo>();
        }
        
        private void PositionEnemyOnLine(OverlayTile tile)
        {
            var tilePos = tile.transform.position;
            _characterInfo.transform.position = new Vector3(tilePos.x, tilePos.y + 0.0001f, tilePos.z);
            _characterInfo.standingOnTile = tile;
        }

    }
}
