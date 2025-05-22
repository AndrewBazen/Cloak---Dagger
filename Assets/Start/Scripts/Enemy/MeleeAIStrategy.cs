using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Start.Scripts.Game;

namespace Start.Scripts.Enemy
{
    /// <summary>
    /// AI strategy for melee enemies.
    /// Focuses on getting close to players and attacking them.
    /// </summary>
    public class MeleeAIStrategy : BaseAIStrategy
    {
        public override Strategy EvaluateStrategy(EnemyController enemy, GameManager gameManager)
        {
            var currentTile = enemy.standingOnTile;
            var bestStrategy = new Strategy();
            
            // Look for players in melee range first
            var playersInMeleeRange = GetPlayersInMeleeRange(currentTile, gameManager);
            if (playersInMeleeRange.Count > 0)
            {
                // We can attack immediately
                var targetPlayer = GetBestMeleeTarget(playersInMeleeRange);
                return new Strategy(
                    currentTile,
                    currentTile,
                    targetPlayer.gameObject,
                    null,
                    CalculateMeleeAttackValue(targetPlayer),
                    0
                );
            }
            
            // If no players in melee range, find the best path to get to a player
            var closestPlayer = GetClosestPlayer(currentTile, gameManager);
            if (closestPlayer != null)
            {
                // Find the best path to get close to the player
                var bestMovementTile = FindBestMovementTile(enemy, closestPlayer, gameManager);
                if (bestMovementTile != null)
                {
                    var path = _pathFinder.FindPath(currentTile, bestMovementTile, new List<OverlayTile>(), false);
                    var movementValue = CalculateMovementValue(path, 1); // Ideal distance is 1 for melee
                    
                    return new Strategy(
                        bestMovementTile,
                        currentTile,
                        closestPlayer.gameObject,
                        null,
                        0, // No attack value yet
                        movementValue
                    );
                }
            }
            
            return bestStrategy; // Return default strategy if no good options found
        }
        
        /// <summary>
        /// Determines the best player to attack in melee range.
        /// Prioritizes lower health and lower armor class.
        /// </summary>
        private CharacterInfo GetBestMeleeTarget(List<CharacterInfo> playersInRange)
        {
            // Simple heuristic: prioritize lower health and lower armor class
            return playersInRange
                .OrderBy(p => p.health)
                .ThenBy(p => p.armorClass)
                .FirstOrDefault();
        }
        
        /// <summary>
        /// Calculates how valuable attacking this player would be.
        /// Higher values are better for attack value.
        /// </summary>
        private float CalculateMeleeAttackValue(CharacterInfo playerInfo)
        {
            // Base attack value is inverse of health (lower health = higher priority)
            float healthFactor = 100f / (playerInfo.health + 10f); // +10 to avoid division by zero
            
            // Armor class affects hit chance
            float armorFactor = 20f / (playerInfo.armorClass + 5f);
            
            // Prioritize targets that are easier to hit and lower health
            return healthFactor * armorFactor * 10f;
        }
        
        /// <summary>
        /// Finds the best tile to move to in order to engage the target player.
        /// </summary>
        private OverlayTile FindBestMovementTile(EnemyController enemy, CharacterInfo targetPlayer, GameManager gameManager)
        {
            var playerTile = targetPlayer.standingOnTile;
            var tilesAroundPlayer = _rangeFinder.GetTilesInRange(playerTile.Grid2DLocation, 1);
            
            // Filter out blocked tiles and tiles with other entities
            var availableTiles = tilesAroundPlayer
                .Where(t => !t.isBlocked && !IsTileOccupied(t, gameManager))
                .ToList();
            
            // Find the closest available tile to the enemy
            return availableTiles
                .OrderBy(t => GetDistance(enemy.standingOnTile, t))
                .FirstOrDefault();
        }
        
        /// <summary>
        /// Checks if a tile is occupied by any character or enemy.
        /// </summary>
        private bool IsTileOccupied(OverlayTile tile, GameManager gameManager)
        {
            // Check if any player is on this tile
            if (gameManager.PartyMembers.Any(p => p.standingOnTile == tile))
                return true;
                
            // Check if any enemy is on this tile
            if (gameManager.Enemies.Any(e => e.standingOnTile == tile))
                return true;
                
            return false;
        }
    }
} 