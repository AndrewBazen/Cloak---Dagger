using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Start.Scripts.AI.Jobs
{
    [BurstCompile]
    public struct RangeFindingJob : IJob
    {
        public int2 StartLocation;
        public int Range;
        public int2 MapSize;
        [ReadOnly] public NativeArray<TileData> MapData;
        public NativeList<int2> ResultTiles; // Stores grid positions of in-range tiles

        public void Execute()
        {
            var visited = new NativeHashSet<int2>(MapData.Length, Allocator.Temp);
            var frontier = new NativeList<int2>(Allocator.Temp);

            visited.Add(StartLocation);
            frontier.Add(StartLocation);
            ResultTiles.Add(StartLocation);

            int step = 0;

            while (step < Range && frontier.Length > 0)
            {
                var nextFrontier = new NativeList<int2>(Allocator.Temp);

                for (int i = 0; i < frontier.Length; i++)
                {
                    var current = frontier[i];
                    foreach (var neighbor in GetCardinalNeighbors(current))
                    {
                        if (!IsInsideBounds(neighbor) || visited.Contains(neighbor))
                            continue;

                        int index = GridToIndex(neighbor);
                        if (index < 0 || index >= MapData.Length)
                            continue;

                        var tile = MapData[index];
                        if (tile.IsBlocked)
                            continue;

                        visited.Add(neighbor);
                        nextFrontier.Add(neighbor);
                        ResultTiles.Add(neighbor);
                    }
                }

                frontier.Dispose();
                frontier = nextFrontier;
                step++;
            }

            frontier.Dispose();
            visited.Dispose();
        }

        private bool IsInsideBounds(int2 pos)
        {
            return pos.x >= 0 && pos.x < MapSize.x && pos.y >= 0 && pos.y < MapSize.y;
        }

        private int GridToIndex(int2 pos)
        {
            return pos.x + pos.y * MapSize.x;
        }

        private static readonly int2[] CardinalOffsets = new int2[]
        {
            new int2(1, 0), new int2(-1, 0), new int2(0, 1), new int2(0, -1)
        };

        private IEnumerable<int2> GetCardinalNeighbors(int2 tilePos)
        {
            foreach (var offset in CardinalOffsets)
            {
                yield return tilePos + offset;
            }
        }
    }

    public struct TileData
    {
        public int2 GridLocation;
        public bool IsBlocked;
    }
}
