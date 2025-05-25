using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Start.Scripts.Game;

namespace Start.Scripts.Enemy.Strategy
{
    /// <summary>
    /// Base abstract class for AI strategies that provides common utilities and methods
    /// for implementing concrete strategies.
    /// </summary>
    public abstract class BaseAIStrategy : IAIStrategy
    {
        protected readonly JobSystemPathFinder _jobSystemPathFinder => JobSystemPathFinder.Instance;
        protected readonly RangeFinder _jobSystemRangeFinder => JobSystemRangeFinder.Instance;
        
        /// <summary>
        /// Evaluates the current game state and determines the best strategy for the enemy.
        /// Each derived class must implement this method.
        /// </summary>
        public abstract Strategy EvaluateStrategy(EnemyController enemy, GameManager gameManager);
        
        /// <summary>
        /// Gets players within a certain range of tiles.
        /// </summary>
        protected List<CharacterInfo> GetPlayersInRange(OverlayTile centerTile, int range, GameManager gameManager)
        {
            var tilesInRange = _rangeFinder.GetTilesInRange(centerTile.Grid2DLocation, range);
            return gameManager.PartyMembers.Where(player => 
                tilesInRange.Contains(player.standingOnTile)).ToList();
        }
        
        /// <summary>
        /// Gets players in melee range (1 tile away) from the specified tile.
        /// </summary>
        protected List<CharacterInfo> GetPlayersInMeleeRange(OverlayTile tile, GameManager gameManager)
        {
            return GetPlayersInRange(tile, 1, gameManager);
        }
        
        /// <summary>
        /// Gets the lowest health player from the provided list.
        /// </summary>
        protected CharacterInfo GetLowestHealthPlayer(List<CharacterInfo> players)
        {
            return players.OrderBy(p => p.health).FirstOrDefault();
        }
        
        /// <summary>
        /// Gets the player closest to the specified tile.
        /// </summary>
        protected CharacterInfo GetClosestPlayer(OverlayTile startTile, GameManager gameManager)
        {
            return gameManager.PartyMembers
                .OrderBy(p => GetDistance(startTile, p.standingOnTile))
                .FirstOrDefault();
        }
        
        /// <summary>
        /// Calculates Manhattan distance between two tiles.
        /// </summary>
        protected int GetDistance(OverlayTile start, OverlayTile end)
        {
            return Mathf.Abs(start.Grid2DLocation.x - end.Grid2DLocation.x) + 
                   Mathf.Abs(start.Grid2DLocation.y - end.Grid2DLocation.y);
        }
        
        /// <summary>
        /// Checks if there is a clear line of sight between two tiles.
        /// </summary>
        protected bool HasLineOfSight(OverlayTile start, OverlayTile end)
        {
            var tilesInLine = _rangeFinder.GetTilesInLine(start.Grid2DLocation, end.Grid2DLocation);
            return !tilesInLine.Any(t => t.isBlocked);
        }
        
        /// <summary>
        /// Calculate a movement value based on path length and target.
        /// Lower is better for movement value.
        /// </summary>
        protected float CalculateMovementValue(List<OverlayTile> path, float desiredDistance)
        {
            if (path == null || path.Count == 0)
                return float.MaxValue;
                
            // Shorter paths are better
            var distanceFactor = path.Count * 0.5f;
            
            // How close to the desired distance we end up
            var distanceOffTarget = Mathf.Abs(path.Count - desiredDistance);
            
            return distanceFactor + distanceOffTarget;
        }
    }
} 
