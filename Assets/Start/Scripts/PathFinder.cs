using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Start.Scripts
{
    public class PathFinder
    {
        private Dictionary<Vector2Int, OverlayTile> _searchableTiles;

        public List<OverlayTile> FindPath(OverlayTile start, OverlayTile end, List<OverlayTile> inRangeTiles, bool isAttack)
        {
            _searchableTiles = new Dictionary<Vector2Int, OverlayTile>();
            
            var openList = new List<OverlayTile>();
            var closedList = new HashSet<OverlayTile>();
            if (!isAttack)
            {
                if (inRangeTiles.Count > 0)
                {
                    foreach (var item in inRangeTiles)
                    {
                        _searchableTiles.Add(item.Grid2DLocation, MapManager.Instance.Map[item.Grid2DLocation]);
                    }
                }
                else
                {
                    _searchableTiles = MapManager.Instance.Map;
                }
    
                openList.Add(start);
    
                while (openList.Any())
                {
                    var currentOverlayTile = openList[0];
    
                    foreach (var tile in openList)
                    {
                        if (tile.F < currentOverlayTile.F ||
                            tile.F == currentOverlayTile.F && tile.h < currentOverlayTile.h)
                        {
                            currentOverlayTile = tile;
                        }
                    }
    
                    openList.Remove(currentOverlayTile);
                    closedList.Add(currentOverlayTile);
    
                    if (currentOverlayTile == end)
                    {
                        return GetFinishedList(start, end);
                    }
    
                    foreach (var neighbor in GetNeightbourOverlayTiles(currentOverlayTile))
                    {
                        if ((neighbor.isBlocked && neighbor != end) || closedList.Contains(neighbor))
                        {
                            continue;
                        }
    
                        neighbor.g = GetManhattenDistance(start, neighbor);
                        neighbor.h = GetManhattenDistance(end, neighbor);
    
                        neighbor.previous = currentOverlayTile;
    
    
                        if (!openList.Contains(neighbor))
                        {
                            openList.Add(neighbor);
                        }
                    }
                }
                return new List<OverlayTile>();
            }
            if (inRangeTiles.Count > 0)
            {
                foreach (var item in inRangeTiles)
                {
                    _searchableTiles.Add(item.Grid2DLocation, MapManager.Instance.Map[item.Grid2DLocation]);
                }
            }
            else
            {
                _searchableTiles = MapManager.Instance.Map;
            }

            openList.Add(start);

            while (openList.Any())
            {
                var currentOverlayTile = openList[0];

                foreach (var tile in openList)
                {
                    if (tile.F < currentOverlayTile.F ||
                        tile.F == currentOverlayTile.F && tile.h < currentOverlayTile.h)
                    {
                        currentOverlayTile = tile;
                    }
                }

                openList.Remove(currentOverlayTile);
                closedList.Add(currentOverlayTile);

                if (currentOverlayTile == end)
                {
                    return GetFinishedList(start, end);
                }

                if (currentOverlayTile != start && currentOverlayTile.isBlocked)
                {
                    return new List<OverlayTile>();
                }

                foreach (var neighbor in GetNeightbourOverlayTiles(currentOverlayTile))
                {
                    if (closedList.Contains(neighbor))
                    {
                        continue;
                    }

                    neighbor.g = GetManhattenDistance(start, neighbor);
                    neighbor.h = GetManhattenDistance(end, neighbor);

                    neighbor.previous = currentOverlayTile;


                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }
            
            return new List<OverlayTile>();
        }
        
        private List<OverlayTile> GetFinishedList(OverlayTile start, OverlayTile end)
        {
            var finishedList = new List<OverlayTile>();
            var currentTile = end;

            while (currentTile != start)
            {
                finishedList.Add(currentTile);
                currentTile = currentTile.previous;
            }

            finishedList.Reverse();

            return finishedList;
        }

        private int GetManhattenDistance(OverlayTile start, OverlayTile tile)
        {
            return Mathf.Abs(start.gridLocation.x - tile.gridLocation.x) + Mathf.Abs(start.gridLocation.y -
                tile.gridLocation.y);
        }

        public List<OverlayTile> GetNeightbourOverlayTiles(OverlayTile currentOverlayTile)
        {
            var map = MapManager.Instance.Map;

            var neighbours = new List<OverlayTile>();

            //right
            var
                locationToCheck = new Vector2Int
                (
                    currentOverlayTile.gridLocation.x + 1,
                    currentOverlayTile.gridLocation.y
                );

            if (_searchableTiles.ContainsKey(locationToCheck))
            {
                neighbours.Add(_searchableTiles[locationToCheck]);
            }

            //left
            locationToCheck = new Vector2Int
            (
                currentOverlayTile.gridLocation.x - 1,
                currentOverlayTile.gridLocation.y
            );

            if (_searchableTiles.ContainsKey(locationToCheck))
            {
                neighbours.Add(_searchableTiles[locationToCheck]);
            }

            //top
            locationToCheck = new Vector2Int
            (
                currentOverlayTile.gridLocation.x,
                currentOverlayTile.gridLocation.y + 1
            );

            if (_searchableTiles.ContainsKey(locationToCheck))
            {
                neighbours.Add(_searchableTiles[locationToCheck]);
            }
        
            //top right
            locationToCheck = new Vector2Int
            (
                currentOverlayTile.gridLocation.x + 1,
                currentOverlayTile.gridLocation.y + 1
            );

            if (_searchableTiles.ContainsKey(locationToCheck))
            {
                neighbours.Add(_searchableTiles[locationToCheck]);
            }
        
            //top left
            locationToCheck = new Vector2Int
            (
                currentOverlayTile.gridLocation.x - 1,
                currentOverlayTile.gridLocation.y + 1
            );

            if (_searchableTiles.ContainsKey(locationToCheck))
            {
                neighbours.Add(_searchableTiles[locationToCheck]);
            }

            //bottom
            locationToCheck = new Vector2Int
            (
                currentOverlayTile.gridLocation.x,
                currentOverlayTile.gridLocation.y - 1
            );

            if (_searchableTiles.ContainsKey(locationToCheck))
            {
                neighbours.Add(_searchableTiles[locationToCheck]);
            }
        
            //bottom right
            locationToCheck = new Vector2Int
            (
                currentOverlayTile.gridLocation.x + 1,
                currentOverlayTile.gridLocation.y - 1
            );

            if (_searchableTiles.ContainsKey(locationToCheck))
            {
                neighbours.Add(_searchableTiles[locationToCheck]);
            }
        
            //bottom left
            locationToCheck = new Vector2Int
            (
                currentOverlayTile.gridLocation.x - 1,
                currentOverlayTile.gridLocation.y - 1
            );

            if (_searchableTiles.ContainsKey(locationToCheck))
            {
                neighbours.Add(_searchableTiles[locationToCheck]);
            }
            
            return neighbours;
        }
    }
}