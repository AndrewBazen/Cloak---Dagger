using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Start.Scripts.Character;

namespace Start.Scripts.Enemy.Strategies
{
    /// <summary>
    /// AI strategy for ranged enemies.
    /// Focuses on maintaining distance and attacking from afar.
    /// </summary>
    public class RangedAIStrategy : BaseAIStrategy
    {
        private const int OptimalRange = 3; // The ideal distance for ranged attacks
        public override Strategy EvaluateStrategy(EnemyController enemy)
        {
            var currentTile = enemy.standingOnTile;
            var bestStrategy = new Strategy();

            // Get weapon range if available
            int attackRange = enemy.weapon != null ? enemy.weapon.weaponRange : 5;

            // Check for players in optimal attack range
            var playersInRange = GetPlayersInRange(currentTile, attackRange);
            if (playersInRange.Count > 0)
            {
                // Filter to players with line of sight
                var targetablePlayers = playersInRange
                    .Where(p => HasLineOfSight(currentTile, p.StandingOnTile))
                    .ToList();
                if (targetablePlayers.Count > 0)
                {
                    // We can attack a player with line of sight
                    var targetPlayer = GetBestRangedTarget(targetablePlayers, currentTile);
                    int distanceToPlayer = GetDistance(currentTile, targetPlayer.StandingOnTile);
                    return new Strategy(
                        currentTile,
                        currentTile,
                        targetPlayer.gameObject,
                        null,
                        CalculateRangedAttackValue(targetPlayer, distanceToPlayer),
                        0
                    );
                }
            }

            // No players in range with line of sight, need to move
            var allPlayers = _gameManager.Party.PartyControllers;
            if (allPlayers.Count > 0)
            {
                // Find the best position to move to
                var bestMovementTile = FindBestRangedPosition(enemy, allPlayers);
                if (bestMovementTile != null)
                {
                    var path = _gameManager.PathFinder.FindPath(currentTile, bestMovementTile, new List<OverlayTile>(), false);
                    if (path != null && path.Count > 0)
                    {
                        var closestPlayer = GetClosestPlayer(bestMovementTile);
                        return new Strategy(
                            bestMovementTile,
                            currentTile,
                            closestPlayer.gameObject,
                            null,
                            0,
                            CalculateMovementValue(path, OptimalRange)
                        );
                    }
                }
            }
            return bestStrategy; // Return default strategy if no good options found
        }
        /// <summary>
        /// Determines the best player to attack with ranged attacks.
        /// Prioritizes line of sight, lower health, and optimal distance.
        /// </summary>
        private PlayerController GetBestRangedTarget(List<PlayerController> playersInRange, OverlayTile currentTile)
        {
            return playersInRange
                .OrderBy(p => p.CurrentHealth) // Target low health first
                .ThenBy(p => Mathf.Abs(GetDistance(currentTile, p.StandingOnTile) - OptimalRange)) // Prefer optimal range
                .FirstOrDefault();
        }
        /// <summary>
        /// Calculates how valuable attacking this player would be from range.
        /// Higher values are better for attack value.
        /// </summary>
        private float CalculateRangedAttackValue(PlayerController playerInfo, int distance)
        {
            // Base attack value is inverse of health (lower health = higher priority)
            float healthFactor = 100f / (playerInfo.CurrentHealth + 10f);

            // Distance factor - optimal at the ideal range
            float distanceFactor = 1f - (Mathf.Abs(distance - OptimalRange) * 0.15f);
            distanceFactor = Mathf.Clamp(distanceFactor, 0.5f, 1f);

            // Armor class affects hit chance
            float armorFactor = 20f / (playerInfo.characterData.armorClass + 5f);
            return healthFactor * distanceFactor * armorFactor * 10f;
        }
        /// <summary>
        /// Finds the best position for a ranged attacker.
        /// Prioritizes:
        /// 1. Line of sight to players
        /// 2. Optimal distance from players
        /// 3. Cover/defensive positions
        /// </summary>
        private OverlayTile FindBestRangedPosition(EnemyController enemy, List<PlayerController> players)
        {
            var currentTile = enemy.standingOnTile;

            // Get tiles at approximately the optimal range from any player
            var candidateTiles = new List<OverlayTile>();
            foreach (var player in players)
            {
                var tilesAroundPlayer = _gameManager.RangeFinder.GetRangeTiles(player.StandingOnTile.Grid2DLocation, OptimalRange + 2);
                candidateTiles.AddRange(tilesAroundPlayer);
            }

            // Remove duplicates, blocked tiles, and occupied tiles
            candidateTiles = candidateTiles.Distinct()
                .Where(t => !t.isBlocked && !IsTileOccupied(t))
                .ToList();

            // Score each tile
            return candidateTiles
                .OrderByDescending(t => ScoreRangedPosition(t, players, currentTile))
                .FirstOrDefault();
        }
        /// <summary>
        /// Scores a position for ranged attack potential.
        /// Higher scores are better.
        /// </summary>
        private float ScoreRangedPosition(OverlayTile tile, List<PlayerController> players, OverlayTile currentTile)
        {
            float score = 0;

            // Count how many players we can see from this position
            int visiblePlayers = 0;
            foreach (var player in players)
            {
                if (HasLineOfSight(tile, player.StandingOnTile))
                {
                    visiblePlayers++;

                    // Add score based on how close this is to optimal range
                    int distance = GetDistance(tile, player.StandingOnTile);
                    float distanceScore = 10f - Mathf.Abs(distance - OptimalRange) * 2f;
                    score += Mathf.Max(0, distanceScore);
                }
            }

            // Big bonus for positions where we can see players
            score += visiblePlayers * 15f;

            // Small penalty for how far we need to move to get here
            int moveDistance = GetDistance(currentTile, tile);
            score -= moveDistance * 0.5f;
            return score;
        }
        /// <summary>
        /// Checks if a tile is occupied by any character or enemy.
        /// </summary>
        private bool IsTileOccupied(OverlayTile tile)
        {
            // Check if any player is on this tile
            if (_gameManager.Party.PartyControllers.Any(p => p.StandingOnTile == tile))
                return true;

            // Check if any enemy is on this tile
            if (_gameManager.Enemies.enemies.Any(e => e.standingOnTile == tile))
                return true;
            return false;
        }
    }
}
