
using UnityEngine;

namespace Start.Scripts
{
    public class SpawnPlayer : MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject playerContainer;
        private CharacterInfo _characterInfo;
        [SerializeField] private int totalPlayers;


        // Update is called once per frame
        private void LateUpdate()
        {
            if (playerContainer.transform.childCount == 0)
            {
                for (var i = 0; i < totalPlayers; i++)
                {
                    SpawnCharacters(MapManager.Instance.playerSpawnTiles[i]);
                }
                
            }
        }

        private void SpawnCharacters(OverlayTile spawnTile)
        {
            if (playerPrefab == null || playerContainer == null)
            {
                Debug.LogError("PlayerPrefab or PlayerContainer is not assigned in the inspector.");
                return;
            }

            var instantiatedPlayer = Instantiate(playerPrefab, playerContainer.transform);
            if (instantiatedPlayer == null)
            {
                Debug.LogError("Failed to instantiate playerPrefab.");
                return;
            }

            _characterInfo = instantiatedPlayer.GetComponent<CharacterInfo>();
            if (_characterInfo == null)
            {
                Debug.LogError("CharacterInfo component is missing on the instantiated playerPrefab.");
                return;
            }

            PositionCharacterOnLine(spawnTile);
        }
           
        private void PositionCharacterOnLine(OverlayTile tile)
        {
            var tilePos = tile.transform.position;
            _characterInfo.transform.position = new Vector3(tilePos.x, tilePos.y + 0.0001f, tilePos.z);
            _characterInfo.standingOnTile = tile;
        }

    }
}