using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Start.Scripts.Map
{
    public class MapManager : MonoBehaviour
    {
        public static MapManager Instance;
        [SerializeField] private GameObject overlayTilePrefab;
        [SerializeField] private GameObject overlayContainer;
        
        public Dictionary<Vector2Int, OverlayTile> Map { get; private set; }
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            Map = new Dictionary<Vector2Int, OverlayTile>();
        }

        private void Start()
        {
            var tileMaps = FindObjectsOfType<Tilemap>();
            var groundMap = tileMaps.First(x => x.gameObject.name == "Ground");
            var originPosition = groundMap.CellToWorld(new Vector3Int(0, 0, 0));
            var gridWidth = groundMap.cellBounds.size.x;
            var gridHeight = groundMap.cellBounds.size.y;

            GenerateOverlayGrid(gridWidth, gridHeight, originPosition, groundMap, tileMaps);
            SetupNeighbors();
        }

        private void GenerateOverlayGrid(int width, int height, Vector3 originPosition, Tilemap groundMap, Tilemap[] allMaps)
        {
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var tilePosition = groundMap.CellToWorld(new Vector3Int(x, y, 0));
                    var overlayTile = Instantiate(overlayTilePrefab, overlayContainer.transform);
                    
                    // Set position and scale based on ground map
                    var cellSize = groundMap.cellSize;
                    overlayTile.transform.position = tilePosition;
                    overlayTile.transform.localScale = new Vector3(cellSize.x, cellSize.y, 1);
                    
                    // Set grid location for pathfinding
                    var tileComponent = overlayTile.GetComponent<OverlayTile>();
                    tileComponent.gridLocation = new Vector2Int(x, y);
                    
                    // Add to our map dictionary
                    Map.Add(new Vector2Int(x, y), tileComponent);
                    
                    // Check if this tile should be blocked (e.g., walls, obstacles)
                    tileComponent.isBlocked = IsPositionBlocked(tilePosition, allMaps);
                }
            }
        }

        private bool IsPositionBlocked(Vector3 position, Tilemap[] maps)
        {
            // Check each tilemap layer for blocking tiles
            foreach (var map in maps)
            {
                if (map.gameObject.CompareTag("Obstacle"))
                {
                    // Convert world position to cell position
                    var cellPosition = map.WorldToCell(position);
                    
                    // Check if there's a tile at this position
                    if (map.HasTile(cellPosition))
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }

        private void SetupNeighbors()
        {
            // For each tile in our map, set up its neighbors for pathfinding
            foreach (var tile in Map.Values)
            {
                var gridPosition = tile.gridLocation;
                
                // Get neighbors in the four cardinal directions
                var north = new Vector2Int(gridPosition.x, gridPosition.y + 1);
                var east = new Vector2Int(gridPosition.x + 1, gridPosition.y);
                var south = new Vector2Int(gridPosition.x, gridPosition.y - 1);
                var west = new Vector2Int(gridPosition.x - 1, gridPosition.y);
                
                // Add cardinal neighbors
                if (Map.ContainsKey(north)) tile.neighbors.Add(Map[north]);
                if (Map.ContainsKey(east)) tile.neighbors.Add(Map[east]);
                if (Map.ContainsKey(south)) tile.neighbors.Add(Map[south]);
                if (Map.ContainsKey(west)) tile.neighbors.Add(Map[west]);
                
                // Optional: Add diagonal neighbors
                var northEast = new Vector2Int(gridPosition.x + 1, gridPosition.y + 1);
                var southEast = new Vector2Int(gridPosition.x + 1, gridPosition.y - 1);
                var southWest = new Vector2Int(gridPosition.x - 1, gridPosition.y - 1);
                var northWest = new Vector2Int(gridPosition.x - 1, gridPosition.y + 1);
                
                if (Map.ContainsKey(northEast)) tile.neighbors.Add(Map[northEast]);
                if (Map.ContainsKey(southEast)) tile.neighbors.Add(Map[southEast]);
                if (Map.ContainsKey(southWest)) tile.neighbors.Add(Map[southWest]);
                if (Map.ContainsKey(northWest)) tile.neighbors.Add(Map[northWest]);
            }
        }

        // Public method to get a tile at a specific grid position
        public OverlayTile GetTileAtPosition(Vector2Int position)
        {
            return Map.ContainsKey(position) ? Map[position] : null;
        }
        
        // Public method to get a tile at a world position
        public OverlayTile GetTileAtWorldPosition(Vector3 worldPosition)
        {
            foreach (var tile in Map.Values)
            {
                if (Vector2.Distance(new Vector2(tile.transform.position.x, tile.transform.position.y), 
                                     new Vector2(worldPosition.x, worldPosition.y)) < 0.3f)
                {
                    return tile;
                }
            }
            return null;
        }
        
        // Clear all visual indicators on tiles
        public void ClearAllTileVisuals()
        {
            foreach (var tile in Map.Values)
            {
                tile.HideTile();
                tile.ResetSprite();
            }
        }
    }
} 