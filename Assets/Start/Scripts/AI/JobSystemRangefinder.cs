using UnityEngine;
using Start.Scripts.Character;
using Unity.Collections;
using Unity.Mathematics;
using Start.Scripts.Map;
using Start.Scripts.AI.Jobs;
using Unity.Jobs;
using System.Collections.Generic;

namespace Start.Scripts.AI
{

    /// <summary>
    /// Handles rangefinding for the player's character in the job system.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    [AddComponentMenu("Job System/Rangefinder")]
    public class JobSystemRangeFinder : MonoBehaviour
    {
        private static JobSystemRangeFinder _instance;
        public static JobSystemRangeFinder Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("JobSystemRangeFinder");
                    _instance = go.AddComponent<JobSystemRangeFinder>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // Grid data cache
        private int2 _gridSize;
        private NativeArray<TileData> _gridData;
        private NativeList<int2> _rangeTiles = new NativeList<int2>(Allocator.TempJob);
        private bool _isGridInitialized;

        // For cache invalidation
        private string _lastMapDataHash;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Initialize or refresh the grid data for pathfinding
        /// </summary>
        public void InitializeGrid(int width, int height, bool[,] obstacleData)
        {
            // Clean up previous data if it exists
            if (_isGridInitialized)
            {
                _gridData.Dispose();
            }

            // Create new grid data
            _gridSize = new int2(width, height);
            _gridData = new NativeArray<TileData>(width * height, Allocator.Persistent);

            // Fill grid data from obstacle map
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    _gridData[y * width + x] = new TileData
                    {
                        IsBlocked = obstacleData[x, y],
                        GridLocation = new int2(x, y)
                    };
                }
            }
            _isGridInitialized = true;
        }

        /// <summary>
        /// Checks if the cached grid data needs to be refreshed
        /// </summary>
        public void CheckGridCache()
        {
            if (MapManager.Instance == null) return;

            // Generate a hash of current map state
            string mapDataHash = GenerateMapHash();
            if (!_isGridInitialized || mapDataHash != _lastMapDataHash)
            {
                RefreshGridFromMap();
                _lastMapDataHash = mapDataHash;
            }
        }

        /// <summary>
        /// Refreshes the grid data from the current map
        /// </summary>
        private void RefreshGridFromMap()
        {
            var map = MapManager.Instance.Map;
            int width = 0;
            int height = 0;

            // Determine grid size
            foreach (var tile in map.Values)
            {
                width = Mathf.Max(width, tile.gridLocation.x + 1);
                height = Mathf.Max(height, tile.gridLocation.y + 1);
            }

            // Create obstacle data
            bool[,] obstacleData = new bool[width, height];

            // Fill obstacle data from map
            foreach (var tile in map.Values)
            {
                int x = tile.gridLocation.x;
                int y = tile.gridLocation.y;
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    obstacleData[x, y] = tile.isBlocked;
                }
            }

            // Initialize the grid
            InitializeGrid(width, height, obstacleData);
        }


        /// <summary>
        /// Gets the range tiles for the player's character.
        /// </summary>
        public List<OverlayTile> GetRangeTiles(Vector2Int location, int movementRange)
        {
            var playerController = GetComponent<PlayerController>();
            if (playerController == null)
            {
                Debug.LogWarning("PlayerController is null. Cannot get range tiles.");
                return new List<OverlayTile>();
            }

            // Ensure the grid is initialized
            CheckGridCache();

            // Get in-range tiles
            if (_isGridInitialized)
            {

                _gridSize = new int2(_gridData.Length, _gridData.Length);
                _gridData = MapManager.ConvertMapToTileData(MapManager.Instance.Map, _gridSize, Allocator.TempJob);
                var rangeFinder = new RangeFindingJob
                {
                    StartLocation = new int2(location.x, location.y),
                    Range = movementRange,
                    ResultTiles = new NativeList<int2>(Allocator.TempJob),
                    MapData = _gridData,
                    MapSize = _gridSize
                };

                // Schedule the job
                var jobHandle = rangeFinder.Schedule();
                jobHandle.Complete();

                // Process the result tiles
                foreach (var tile in rangeFinder.ResultTiles)
                {
                    var overlayTile = MapManager.Instance.GetTileAtPosition(new Vector2Int(tile.x, tile.y));
                    if (overlayTile != null)
                    {
                        overlayTile.ShowTile();
                    }
                }
                var returnTiles = ConvertToOverlayTiles(rangeFinder.ResultTiles);
                rangeFinder.ResultTiles.Dispose();
                return returnTiles;
            }
            else
            {
                Debug.LogWarning("Grid data is not initialized. Cannot get range tiles.");
                return new List<OverlayTile>();
            }
        }

        private List<OverlayTile> ConvertToOverlayTiles(NativeList<int2> nativeTiles)
        {
            var overlayTiles = new List<OverlayTile>();
            foreach (var tile in nativeTiles)
            {
                var overlayTile = MapManager.Instance.GetTileAtPosition(new Vector2Int(tile.x, tile.y));
                if (overlayTile != null)
                {
                    overlayTiles.Add(overlayTile);
                }
            }
            return overlayTiles;
        }

        /// <summary>
        /// Generate a simple hash to detect map changes
        /// </summary>
        private string GenerateMapHash()
        {
            if (MapManager.Instance == null || MapManager.Instance.Map == null)
                return "";
            var map = MapManager.Instance.Map;
            int hash = 0;
            foreach (var tile in map.Values)
            {
                // Combine position and blocked state into hash
                hash = hash * 31 + tile.gridLocation.x;
                hash = hash * 31 + tile.gridLocation.y;
                hash = hash * 31 + (tile.isBlocked ? 1 : 0);
            }
            return hash.ToString();
        }
    }
}
