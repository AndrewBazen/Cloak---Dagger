using UnityEngine;

namespace Start.Scripts
{
    public class SpawnPlayer : MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject playerContainer;
        private CharacterInfo _characterInfo;

        // Update is called once per frame
        void Update()
        {
            if (_characterInfo == null)
            {
                SpawnCharacter();
                PositionCharacterOnLine(MapManager.Instance.playerSpawnTile);
            }
        }
        
        private void SpawnCharacter()
        {
            _characterInfo = Instantiate(playerPrefab, playerContainer.transform).GetComponent<CharacterInfo>();
            PositionCharacterOnLine(MapManager.Instance.playerSpawnTile);
        }
        
        private void PositionCharacterOnLine(OverlayTile tile)
        {
            var tilePos = tile.transform.position;
            _characterInfo.transform.position = new Vector3(tilePos.x, tilePos.y + 0.0001f, tilePos.z);
            _characterInfo.standingOnTile = tile;
        }

    }
}