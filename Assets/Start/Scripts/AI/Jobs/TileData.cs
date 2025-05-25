using Unity.Mathematics;

namespace Start.Scripts.AI.Jobs
{
    public struct TileData
    {
        public int2 GridLocation;
        public bool IsBlocked;
        public float MovementCost;
    }
}

