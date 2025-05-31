using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Start.Scripts.AI.Jobs
{
    /// <summary>
    /// A job that performs A* pathfinding using the Unity Job System for multi-threading.
    /// Works with a fixed grid of nodes and calculates optimal paths.
    /// </summary>
    [BurstCompile]
    public struct PathfindingJob : IJob
    {
        // Input data
        [ReadOnly] public NativeArray<int> GridData;
        [ReadOnly] public int2 GridSize;
        [ReadOnly] public int2 StartPosition;
        [ReadOnly] public int2 EndPosition;

        // Output path
        public NativeList<int2> Path;

        // Constants for pathfinding
        private const int NodeOpen = 0;
        private const int NodeClosed = 1;
        private const int NodeBlocked = 2;

        // Internal data for pathfinding
        private NativeArray<int> _nodeStates;
        private NativeArray<int> _gCosts;
        private NativeArray<int> _hCosts;
        private NativeArray<int> _fCosts;
        private NativeArray<int2> _parentNodes;
        private NativeList<int2> _openSet;


        private static readonly int2[] CardinalDirections = new int2[]
        {
            new int2(0, 1), new int2(1, 0), new int2(0, -1), new int2(-1, 0)
        };
        public void Execute()
        {
            // Initialize arrays for pathfinding
            InitializeData();

            // No path exists if start or end position is blocked
            if (IsNodeBlocked(StartPosition) || IsNodeBlocked(EndPosition))
            {
                return;
            }

            // Add start node to open set
            _openSet.Add(StartPosition);
            SetNodeState(StartPosition, NodeOpen);

            // A* algorithm
            while (_openSet.Length > 0)
            {
                // Get node with lowest F cost
                int2 current = GetNodeWithLowestFCost();

                // If we've reached the end position, construct the path
                if (current.Equals(EndPosition))
                {
                    ConstructPath(current);
                    break;
                }

                // Remove current from open set and add to closed set
                RemoveFromOpenSet(current);
                SetNodeState(current, NodeClosed);

                // Check all neighboring nodes
                ProcessNeighbors(current);
            }

            // Clean up
            DisposeTemporaryData();
        }
        private void InitializeData()
        {
            int totalNodes = GridSize.x * GridSize.y;
            _nodeStates = new NativeArray<int>(totalNodes, Allocator.Temp);
            _gCosts = new NativeArray<int>(totalNodes, Allocator.Temp);
            _hCosts = new NativeArray<int>(totalNodes, Allocator.Temp);
            _fCosts = new NativeArray<int>(totalNodes, Allocator.Temp);
            _parentNodes = new NativeArray<int2>(totalNodes, Allocator.Temp);
            _openSet = new NativeList<int2>(totalNodes, Allocator.Temp);

            // Initialize states from grid data
            for (int y = 0; y < GridSize.y; y++)
            {
                for (int x = 0; x < GridSize.x; x++)
                {
                    int2 pos = new int2(x, y);
                    int index = GetIndex(pos);
                    if (GridData[index] == 1)
                    {
                        // 1 means blocked in the grid data
                        SetNodeState(pos, NodeBlocked);
                    }

                    // Initialize costs to a very large value
                    _gCosts[index] = int.MaxValue;
                    _hCosts[index] = int.MaxValue;
                    _fCosts[index] = int.MaxValue;
                }
            }

            // Set starting node costs
            int startIndex = GetIndex(StartPosition);
            _gCosts[startIndex] = 0;
            _hCosts[startIndex] = CalculateHCost(StartPosition, EndPosition);
            _fCosts[startIndex] = _hCosts[startIndex];
        }
        private void DisposeTemporaryData()
        {
            _nodeStates.Dispose();
            _gCosts.Dispose();
            _hCosts.Dispose();
            _fCosts.Dispose();
            _parentNodes.Dispose();
            _openSet.Dispose();
        }
        private int2 GetNodeWithLowestFCost()
        {
            int lowestCost = int.MaxValue;
            int2 lowestNode = _openSet[0];
            for (int i = 0; i < _openSet.Length; i++)
            {
                int2 node = _openSet[i];
                int fCost = _fCosts[GetIndex(node)];
                if (fCost < lowestCost)
                {
                    lowestCost = fCost;
                    lowestNode = node;
                }
            }
            return lowestNode;
        }
        private void RemoveFromOpenSet(int2 node)
        {
            for (int i = 0; i < _openSet.Length; i++)
            {
                if (_openSet[i].Equals(node))
                {
                    _openSet.RemoveAt(i);
                    break;
                }
            }
        }
        private void ProcessNeighbors(int2 current)
        {
            for (int i = 0; i < CardinalDirections.Length; i++)
            {
                int2 dir = CardinalDirections[i];

                int2 neighbor = current + dir;

                // Skip if out of bounds or blocked or already in closed set
                if (!IsValidPosition(neighbor) || IsNodeBlocked(neighbor) || GetNodeState(neighbor) == NodeClosed)
                {
                    continue;
                }

                // Calculate new G cost
                int newGCost = _gCosts[GetIndex(current)] + 10; // 10 for cardinal movement cost

                // If the neighbor node is not in the open set or we've found a better path
                if (GetNodeState(neighbor) != NodeOpen || newGCost < _gCosts[GetIndex(neighbor)])
                {
                    // Update costs
                    int neighborIndex = GetIndex(neighbor);
                    _gCosts[neighborIndex] = newGCost;
                    _hCosts[neighborIndex] = CalculateHCost(neighbor, EndPosition);
                    _fCosts[neighborIndex] = _gCosts[neighborIndex] + _hCosts[neighborIndex];

                    // Set parent for path reconstruction
                    _parentNodes[neighborIndex] = current;

                    // Add to open set if not already there
                    if (GetNodeState(neighbor) != NodeOpen)
                    {
                        _openSet.Add(neighbor);
                        SetNodeState(neighbor, NodeOpen);
                    }
                }
            }
        }
        private void ConstructPath(int2 endPos)
        {
            // Clear the output path list
            Path.Clear();

            // Start from the end position and follow parent pointers back to start
            int2 current = endPos;

            // Add end position first (we'll reverse it later)
            Path.Add(current);

            // Keep adding parent nodes until we reach the start position
            while (!current.Equals(StartPosition))
            {
                current = _parentNodes[GetIndex(current)];
                Path.Add(current);
            }

            // Reverse the path so it goes from start to end
            // Unfortunately, NativeList doesn't have a built-in reverse function,
            // so we need to do it manually
            for (int i = 0; i < Path.Length / 2; i++)
            {
                int2 temp = Path[i];
                Path[i] = Path[Path.Length - 1 - i];
                Path[Path.Length - 1 - i] = temp;
            }
        }
        private int CalculateHCost(int2 a, int2 b)
        {
            // Manhattan distance
            return math.abs(a.x - b.x) + math.abs(a.y - b.y);
        }
        private int GetIndex(int2 position)
        {
            return position.y * GridSize.x + position.x;
        }
        private bool IsValidPosition(int2 position)
        {
            return position.x >= 0 && position.x < GridSize.x &&
                   position.y >= 0 && position.y < GridSize.y;
        }
        private bool IsNodeBlocked(int2 position)
        {
            return GetNodeState(position) == NodeBlocked;
        }
        private int GetNodeState(int2 position)
        {
            return _nodeStates[GetIndex(position)];
        }
        private void SetNodeState(int2 position, int state)
        {
            _nodeStates[GetIndex(position)] = state;
        }
    }
}
