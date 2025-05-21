using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Start.Scripts
{
    public class RangeFinder
    {
        public List<OverlayTile> GetTilesInRange(Vector2Int location, int range)
        {
            var startingTile = MapManager.Instance.Map[location];
            var inRangeTiles = new List<OverlayTile>();
            int stepCount = 0;

            inRangeTiles.Add(startingTile);

            //Should contain the surroundingTiles of the previous step. 
            var tilesForPreviousStep = new List<OverlayTile> { startingTile };
            while (stepCount < range)
            {
                var surroundingTiles = new List<OverlayTile>();

                foreach (var item in tilesForPreviousStep)
                {
                    if (range > 1)
                    {
                        surroundingTiles.AddRange(MapManager.Instance.GetSurroundingTiles
                            (new Vector2Int(item.gridLocation.x, item.gridLocation.y)));
                        continue;
                    }
                    surroundingTiles.AddRange(MapManager.Instance.GetAllSurroundingTiles
                        (new Vector2Int(item.gridLocation.x, item.gridLocation.y)));
                }

                inRangeTiles.AddRange(surroundingTiles);
                tilesForPreviousStep = surroundingTiles.Distinct().ToList();
                stepCount++;
            }

            return inRangeTiles.Distinct().ToList();
        }
    }
}