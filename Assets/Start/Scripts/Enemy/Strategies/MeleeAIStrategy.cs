using System.Collections.Generic;
using System.Linq;
using Start.Scripts.Character;

namespace Start.Scripts.Enemy.Strategies
{
    /// <summary>
    /// AI strategy for melee enemies.
    /// Focuses on getting close to players and attacking them.
    /// </summary>
    public class MeleeAIStrategy : BaseAIStrategy
    {
        public override Strategy EvaluateStrategy(EnemyController enemy)
        {
            var currentTile = enemy.standingOnTile;
            var bestStrategy = new Strategy();

            // Look for players in melee range first
            var playersInMeleeRange = GetPlayersInMeleeRange(currentTile);
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
            var closestPlayer = GetClosestPlayer(currentTile);
            if (closestPlayer != null)
            {
                // Find the best path to get close to the player
                var bestMovementTile = FindBestMovementTile(enemy, closestPlayer);
                if (bestMovementTile != null)
                {
                    var path = _gameManager.PathFinder.FindPath(currentTile, bestMovementTile, new List<OverlayTile>(), false);
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
        private PlayerController GetBestMeleeTarget(List<PlayerController> playersInRange)
        {
            // Simple heuristic: prioritize lower health and lower armor class
            return playersInRange
                .OrderBy(p => p.CurrentHealth)
                .ThenBy(p => p.characterData.armorClass)
                .FirstOrDefault();
        }

        //private void WeaponBehavior()
        //{
        //    var weapon = _gameManager.Item.ItemDatabase.GetItem(enemy.data.weaponId);
        //    if (weapon != null && weapon.aiHints != null)
        //    {
        //        if (weapon.aiHints.prefersCover)
        //            SeekCover();
        //
        //        if (weapon.aiHints.avoidsMelee)
        //            AvoidCloseQuarters();
        //    }
        //
        //}

        //private void SeekCover(EnemyController enemy)
        //}
        //   // Logic to find cover tiles and move towards them
        //   var coverTiles = _gameManager.RangeFinder.GetCoverTiles(enemy.standingOnTile.Grid2DLocation, 2);
        //   if (coverTiles.Count > 0)
        //   {
        //       var bestCoverTile = coverTiles.OrderBy(t => GetDistance(enemy.standingOnTile, t)).FirstOrDefault();
        //       if (bestCoverTile != null)
        //       {
        //           // Move to the best cover tile
        //           _gameManager.PathFinder.FindPath(enemy.standingOnTile, bestCoverTile, new List<OverlayTile>(), false);
        //       }
        //   }
        // }
        /// <summary>
        /// Calculates how valuable attacking this player would be.
        /// Higher values are better for attack value.
        /// </summary>
        private float CalculateMeleeAttackValue(PlayerController playerInfo)
        {
            // Base attack value is inverse of health (lower health = higher priority)
            float healthFactor = 100f / (playerInfo.CurrentHealth + 10f); // +10 to avoid division by zero

            // Armor class affects hit chance
            float armorFactor = 20f / (playerInfo.characterData.armorClass + 5f);

            // Prioritize targets that are easier to hit and lower health
            return healthFactor * armorFactor * 10f;
        }
        /// <summary>
        /// Finds the best tile to move to in order to engage the target player.
        /// </summary>
        private OverlayTile FindBestMovementTile(EnemyController enemy, PlayerController targetPlayer)
        {
            var playerTile = targetPlayer.StandingOnTile;
            var tilesAroundPlayer = _gameManager.RangeFinder.GetRangeTiles(playerTile.Grid2DLocation, 1);

            // Filter out blocked tiles and tiles with other entities
            var availableTiles = tilesAroundPlayer
                .Where(t => !t.isBlocked && !IsTileOccupied(t))
                .ToList();

            // Find the closest available tile to the enemy
            return availableTiles
                .OrderBy(t => GetDistance(enemy.standingOnTile, t))
                .FirstOrDefault();
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
