using System;
using System.Collections.Generic;
using Start.Scripts.AI.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Start.Scripts.AI
{
    /// <summary>
    /// Provides pathfinding functionality using Unity's Job System for parallel processing.
    /// This is a more efficient replacement for the original PathFinder class.
    /// </summary>
    public class JobSystemPathFinder : MonoBehaviour
    {
        private static JobSystemPathFinder _instance;
        
        public static JobSystemPathFinder Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("JobSystemPathFinder");
                    _instance = go.AddComponent<JobSystemPathFinder>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // Grid data cache
        private int2 _gridSize;
        private NativeArray<int> _gridData;
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
            _gridData = new NativeArray<int>(width * height, Allocator.Persistent);
            
            // Fill grid data from obstacle map
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    _gridData[y * width + x] = obstacleData[x, y] ? 1 : 0;
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
        /// Find a path from start to target tile using the Job System
        /// </summary>
        public List<OverlayTile> FindPath(OverlayTile startTile, OverlayTile targetTile, List<OverlayTile> tilesInRange, bool ignoreObstacles = false)
        {
            // Ensure grid is up to date
            CheckGridCache();
            
            if (!_isGridInitialized)
            {
                Debug.LogError("Pathfinding grid is not initialized!");
                return new List<OverlayTile>();
            }
            
            // Convert to grid positions
            int2 startPos = new int2(startTile.gridLocation.x, startTile.gridLocation.y);
            int2 endPos = new int2(targetTile.gridLocation.x, targetTile.gridLocation.y);
            
            // Check if positions are valid
            if (!IsValidPosition(startPos) || !IsValidPosition(endPos))
            {
                Debug.LogWarning("Pathfinding positions out of bounds!");
                return new List<OverlayTile>();
            }
            
            // Create a copy of the grid for this path calculation
            // This allows for temporary modifications (like ignoring obstacles)
            NativeArray<int> pathGridData = new NativeArray<int>(_gridData, Allocator.TempJob);
            
            // If ignoring obstacles, clear obstacles along the path
            if (ignoreObstacles)
            {
                for (int i = 0; i < pathGridData.Length; i++)
                {
                    if (pathGridData[i] == 1)
                    {
                        pathGridData[i] = 0;
                    }
                }
            }
            
            // Create output container for the path
            NativeList<int2> pathPositions = new NativeList<int2>(Allocator.TempJob);
            
            // Create and schedule the pathfinding job
            var pathfindingJob = new PathfindingJob
            {
                GridData = pathGridData,
                GridSize = _gridSize,
                StartPosition = startPos,
                EndPosition = endPos,
                Path = pathPositions
            };
            
            // Execute job immediately (synchronously)
            pathfindingJob.Execute();
            
            // Convert path positions back to OverlayTiles
            List<OverlayTile> path = new List<OverlayTile>();
            if (pathPositions.Length > 0)
            {
                // Skip the first position (start position)
                for (int i = 1; i < pathPositions.Length; i++)
                {
                    int2 pos = pathPositions[i];
                    Vector2Int gridPos = new Vector2Int(pos.x, pos.y);
                    OverlayTile tile = MapManager.Instance.GetTileAtPosition(gridPos);
                    
                    if (tile != null)
                    {
                        path.Add(tile);
                    }
                }
            }
            
            // Clean up native collections
            pathGridData.Dispose();
            pathPositions.Dispose();
            
            return path;
        }
        
        /// <summary>
        /// Check if a position is within the grid bounds
        /// </summary>
        private bool IsValidPosition(int2 position)
        {
            return position.x >= 0 && position.x < _gridSize.x && 
                   position.y >= 0 && position.y < _gridSize.y;
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

        private void OnDestroy()
        {
            if (_isGridInitialized && _gridData.IsCreated)
            {
                _gridData.Dispose();
            }
            
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
} 