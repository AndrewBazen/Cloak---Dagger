using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Start.Scripts
{
    public class MapManager : MonoBehaviour
    {
        private static MapManager _instance;
        public static MapManager Instance => _instance;

        private List<GameObject> _playerSpawnPoints;
        private List<GameObject> _enemySpawnPoints;
        public List<OverlayTile> enemySpawnTiles;
        public List<OverlayTile> playerSpawnTiles;
        public GameObject overlayPrefab;
        public GameObject overlayContainer;
        public Dictionary<Vector2Int, OverlayTile> Map;
        public bool ignoreBottomTiles;
        public bool gameOver;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            } else
            {
                _instance = this;
            }
            
        }

        void Start()
        {
            gameOver = false;
            _enemySpawnPoints = GameObject.FindGameObjectsWithTag("enemySpawn").ToList();
            var enemySpawnLocations = new List<Vector3>();
            foreach (var esp in _enemySpawnPoints)
            {
                var enemyLocation = esp.transform.position;
                var enemySpawn = new Vector3(enemyLocation.x, enemyLocation.y, 
                                                     enemyLocation.z);
                enemySpawnLocations.Add(enemySpawn);
                
            }

            _playerSpawnPoints = GameObject.FindGameObjectsWithTag("playerSpawn").ToList();
            var playerSpawnLocations = new List<Vector3>();
            foreach (var psp in _playerSpawnPoints)
            {
                var playerLocation = psp.transform.position;
                playerSpawnLocations.Add(playerLocation);
            }
            var tileMaps =
                gameObject.transform.GetComponentsInChildren<Tilemap>().OrderByDescending(x =>
                    x.GetComponent<TilemapRenderer>().sortingOrder);
            Map = new Dictionary<Vector2Int, OverlayTile>();

            foreach (var tm in tileMaps)
            {
                BoundsInt bounds = tm.cellBounds;

                for (int z = bounds.max.z; z >= bounds.min.z; z--)
                {
                    for (int y = bounds.min.y; y < bounds.max.y; y++)
                    {
                        for (int x = bounds.min.x; x < bounds.max.x; x++)
                        {

                            if (z == 0 && ignoreBottomTiles)
                                return;

                            if (tm.HasTile(new Vector3Int(x, y, z)))
                            {
                                if (!Map.ContainsKey(new Vector2Int(x, y)))
                                {
                                    var overlayTile = Instantiate(overlayPrefab, overlayContainer.transform);
                                    var cellWorldPosition = tm.GetCellCenterWorld(new Vector3Int(x, y, z));
                                    overlayTile.transform.position = new Vector3(cellWorldPosition.x,
                                        cellWorldPosition.y, cellWorldPosition.z + 1);
                                    overlayTile.GetComponent<SpriteRenderer>().sortingOrder = 
                                        tm.GetComponent<TilemapRenderer>().sortingOrder;
                                    overlayTile.gameObject.GetComponent<OverlayTile>().gridLocation = 
                                        new Vector3Int(x, y, z);
                                    Vector3 tilePosition = overlayTile.transform.position;
                                    foreach (var psl in playerSpawnLocations)
                                    {
                                        if (Mathf.Abs(tilePosition.x - psl.x) < 0.2f && 
                                            Mathf.Abs(tilePosition.y - psl.y) < 0.2f)
                                        {
                                            overlayTile.gameObject.GetComponent<OverlayTile>().isPlayerSpawnTile = true;
                                            playerSpawnTiles.Add(overlayTile.gameObject.GetComponent<OverlayTile>());
                                        }
                                    }
                                    

                                    foreach (var esl in enemySpawnLocations)
                                    {
                                        if (Mathf.Abs(tilePosition.x - esl.x) < 0.2f && Mathf.Abs(tilePosition.y - 
                                                esl.y) < 0.2f)
                                        {
                                            overlayTile.gameObject.GetComponent<OverlayTile>().isEnemySpawnTile = true;
                                            enemySpawnTiles.Add(overlayTile.gameObject.GetComponent<OverlayTile>());
                                        }
                                    }
                                    
                                    Map.Add(new Vector2Int(x, y), overlayTile.gameObject.GetComponent<OverlayTile>());
                                }
                            }
                        }
                    }
                }
            }
        }

        public List<OverlayTile> GetSurroundingTiles(Vector2Int originTile)
        {
            var surroundingTiles = new List<OverlayTile>();
        
        
            var tileToCheck = new Vector2Int(originTile.x + 1, originTile.y);
            if (Map.ContainsKey(tileToCheck))
            {
                if (Mathf.Abs(Map[tileToCheck].transform.position.z - Map[originTile].transform.position.z) <= 1 &&
                    !surroundingTiles.Contains(Map[tileToCheck]))
                    surroundingTiles.Add(Map[tileToCheck]);
            }
        
            tileToCheck = new Vector2Int(originTile.x - 1, originTile.y);
            if (Map.ContainsKey(tileToCheck))
            {
                if (Mathf.Abs(Map[tileToCheck].transform.position.z - Map[originTile].transform.position.z) <= 1 &&
                    !surroundingTiles.Contains(Map[tileToCheck]))
                    surroundingTiles.Add(Map[tileToCheck]);
            }
        
            tileToCheck = new Vector2Int(originTile.x, originTile.y + 1);
            if (Map.ContainsKey(tileToCheck))
            {
                if (Mathf.Abs(Map[tileToCheck].transform.position.z - Map[originTile].transform.position.z) <= 1 &&
                    !surroundingTiles.Contains(Map[tileToCheck]))
                    surroundingTiles.Add(Map[tileToCheck]);
            }
        
            tileToCheck = new Vector2Int(originTile.x, originTile.y - 1);
            if (Map.ContainsKey(tileToCheck))
            {
                if (Mathf.Abs(Map[tileToCheck].transform.position.z - Map[originTile].transform.position.z) <= 1 &&
                    !surroundingTiles.Contains(Map[tileToCheck]))
                    surroundingTiles.Add(Map[tileToCheck]);
            }
            
            return surroundingTiles;
        }

        public List<OverlayTile> GetAllSurroundingTiles(Vector2Int originTile)
        {
            var surroundingTiles = new List<OverlayTile>();
        
        
            var tileToCheck = new Vector2Int(originTile.x + 1, originTile.y);
            if (Map.ContainsKey(tileToCheck))
            {
                if (Mathf.Abs(Map[tileToCheck].transform.position.z - Map[originTile].transform.position.z) <= 1 &&
                    !surroundingTiles.Contains(Map[tileToCheck]))
                    surroundingTiles.Add(Map[tileToCheck]);
            }
        
            tileToCheck = new Vector2Int(originTile.x - 1, originTile.y);
            if (Map.ContainsKey(tileToCheck))
            {
                if (Mathf.Abs(Map[tileToCheck].transform.position.z - Map[originTile].transform.position.z) <= 1 &&
                    !surroundingTiles.Contains(Map[tileToCheck]))
                    surroundingTiles.Add(Map[tileToCheck]);
            }
        
            tileToCheck = new Vector2Int(originTile.x, originTile.y + 1);
            if (Map.ContainsKey(tileToCheck))
            {
                if (Mathf.Abs(Map[tileToCheck].transform.position.z - Map[originTile].transform.position.z) <= 1 &&
                    !surroundingTiles.Contains(Map[tileToCheck]))
                    surroundingTiles.Add(Map[tileToCheck]);
            }
        
            tileToCheck = new Vector2Int(originTile.x, originTile.y - 1);
            if (Map.ContainsKey(tileToCheck))
            {
                if (Mathf.Abs(Map[tileToCheck].transform.position.z - Map[originTile].transform.position.z) <= 1 &&
                    !surroundingTiles.Contains(Map[tileToCheck]))
                    surroundingTiles.Add(Map[tileToCheck]);
            }
            
            tileToCheck = new Vector2Int(originTile.x + 1, originTile.y + 1);
            if (Map.ContainsKey(tileToCheck))
            {
                if (Mathf.Abs(Map[tileToCheck].transform.position.z - Map[originTile].transform.position.z) <= 1 &&
                    !surroundingTiles.Contains(Map[tileToCheck]))
                    surroundingTiles.Add(Map[tileToCheck]);
            }
        
            tileToCheck = new Vector2Int(originTile.x - 1, originTile.y + 1);
            if (Map.ContainsKey(tileToCheck))
            {
                if (Mathf.Abs(Map[tileToCheck].transform.position.z - Map[originTile].transform.position.z) <= 1 &&
                    !surroundingTiles.Contains(Map[tileToCheck]))
                    surroundingTiles.Add(Map[tileToCheck]);
            }
        
            tileToCheck = new Vector2Int(originTile.x + 1, originTile.y - 1);
            if (Map.ContainsKey(tileToCheck))
            {
                if (Mathf.Abs(Map[tileToCheck].transform.position.z - Map[originTile].transform.position.z) <= 1 &&
                    !surroundingTiles.Contains(Map[tileToCheck]))
                    surroundingTiles.Add(Map[tileToCheck]);
            }
        
            tileToCheck = new Vector2Int(originTile.x - 1, originTile.y - 1);
            if (Map.ContainsKey(tileToCheck))
            {
                if (Mathf.Abs(Map[tileToCheck].transform.position.z - Map[originTile].transform.position.z) <= 1 &&
                    !surroundingTiles.Contains(Map[tileToCheck]))
                    surroundingTiles.Add(Map[tileToCheck]);
            }
            
            return surroundingTiles;
        }
    }
}

