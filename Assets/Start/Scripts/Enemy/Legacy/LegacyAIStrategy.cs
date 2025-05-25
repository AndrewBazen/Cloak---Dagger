// LegacyAIStrategyExamples.cs
// Archived: Previous strategy logic for Melee and Ranged AI.
// Retained for future reference, debugging, or conversion.

using UnityEngine;

namespace Start.Scripts.Enemy.Legacy
{
    public static class LegacyAIStrategy
    {
        public static void DetermineBestStrategy()
        {
          
            // create new strategy
            var strategy = new Strategy();
        
            // if enemy is a melee type
            switch (enemyType)
            {
                case "Melee":
                {
                    // check all tiles in range of current position
                    var tempStrategy = new Strategy();
                    var playersInRange = GetPlayersInRange();
                    if (playersInRange.Count > 0)
                    {
                        switch (playersInRange.Count)
                        {
                            case 1:
                            {
                                var playerTile = PlayerDict[playersInRange[0]].standingOnTile;
                                var inRangeTiles = _rangeFinder.GetTilesInRange(playerTile.Grid2DLocation, 1);
                                if (inRangeTiles.Contains(playerTile))
                                    inRangeTiles.Remove(playerTile);
                                foreach (var tile in inRangeTiles)
                                {
                                    if (tile.isBlocked)
                                    {
                                        continue;
                                    }
                                    var tempMovePath = _pathFinder.FindPath(standingOnTile, tile,
                                        new List<OverlayTile>(), false);
                                    var checkTiles = _rangeFinder.GetTilesInRange(tile.Grid2DLocation, 2);
                                    var checkedPlayers = GetPlayersInRange(checkTiles);
                                    if (checkedPlayers.Count > 1)
                                    {
                                        checkedPlayers.Remove(playersInRange[0]);
                                        tempStrategy.StrategyMovementValue =
                                            CalculateMovementValue(tempMovePath) - checkedPlayers.Count;
                                        tempStrategy.StrategyAttackValue =
                                            CalculateMeleeAttackValue(PlayerDict[playersInRange[0]]);
                                        tempStrategy.TargetTile = tile;
                                        tempStrategy.CurrentTile = standingOnTile;
                                        tempStrategy.PlayerToAttack = playersInRange[0];
                                        if (tempStrategy.StrategyValue > strategy.StrategyValue)
                                        {
                                            strategy = new Strategy(tempStrategy.TargetTile,
                                                tempStrategy.CurrentTile, tempStrategy.PlayerToAttack, tempStrategy.AbilityToUse, tempStrategy.StrategyAttackValue,
                                                tempStrategy.StrategyMovementValue);
                                        }
                                    }
                                    tempStrategy.StrategyMovementValue =
                                        CalculateMovementValue(tempMovePath);
                                    tempStrategy.StrategyAttackValue =
                                        CalculateMeleeAttackValue(PlayerDict[playersInRange[0]]);
                                    tempStrategy.TargetTile = tile;
                                    tempStrategy.CurrentTile = standingOnTile;
                                    tempStrategy.PlayerToAttack = playersInRange[0];
                                    if (tempStrategy.StrategyValue > strategy.StrategyValue)
                                    {
                                        strategy = new Strategy(tempStrategy.TargetTile,
                                                tempStrategy.CurrentTile, tempStrategy.PlayerToAttack, tempStrategy.AbilityToUse, tempStrategy.StrategyAttackValue,
                                                tempStrategy.StrategyMovementValue);
                                    }
                                }
                                Debug.Log(strategy);
                                break;
                            }
                            case > 1:
                            {
                                var bestPlayer = GetBestPlayer(playersInRange);
                                if (!bestPlayer)
                                    break;
                                var bestPlayerTile = PlayerDict[bestPlayer].standingOnTile;
                                var inRangeTiles = _rangeFinder.GetTilesInRange(bestPlayerTile.Grid2DLocation, 1);
                                if (inRangeTiles.Contains(bestPlayerTile))
                                    inRangeTiles.Remove(bestPlayerTile);
                                foreach (var tile in inRangeTiles)
                                {
                                    if (tile.isBlocked)
                                    {
                                        continue;
                                    }
                                    var tempMovePath = _pathFinder.FindPath(standingOnTile, tile,
                                        new List<OverlayTile>(), false);
                                    var checkTiles = _rangeFinder.GetTilesInRange(tile.Grid2DLocation, 2);
                                    var checkedPlayers = GetPlayersInRange(checkTiles);
                                    if (checkedPlayers.Count > 1)
                                    {
                                        checkedPlayers.Remove(bestPlayer);
                                        tempStrategy.StrategyMovementValue =
                                            CalculateMovementValue(tempMovePath) - checkedPlayers.Count;
                                        tempStrategy.StrategyAttackValue =
                                            CalculateMeleeAttackValue(PlayerDict[bestPlayer]);
                                        tempStrategy.TargetTile = tile;
                                        tempStrategy.CurrentTile = standingOnTile;
                                        tempStrategy.PlayerToAttack = bestPlayer;
                                        if (tempStrategy.StrategyValue >
                                            strategy.StrategyValue)
                                        {
                                            strategy = new Strategy(tempStrategy.TargetTile,
                                                tempStrategy.CurrentTile, tempStrategy.PlayerToAttack, tempStrategy.AbilityToUse, tempStrategy.StrategyAttackValue,
                                                tempStrategy.StrategyMovementValue);
                                        }
                                    }
                                    tempStrategy.StrategyMovementValue =
                                        CalculateMovementValue(tempMovePath);
                                    tempStrategy.StrategyAttackValue =
                                        CalculateMeleeAttackValue(PlayerDict[bestPlayer]);
                                    tempStrategy.TargetTile = tile;
                                    tempStrategy.CurrentTile = standingOnTile;
                                    tempStrategy.PlayerToAttack = bestPlayer;
                                    if (tempStrategy.StrategyValue > strategy.StrategyValue)
                                    {
                                        strategy = new Strategy(tempStrategy.TargetTile,
                                                tempStrategy.CurrentTile, tempStrategy.PlayerToAttack, tempStrategy.AbilityToUse, tempStrategy.StrategyAttackValue,
                                                tempStrategy.StrategyMovementValue);
                                    }
                                }
                                Debug.Log(strategy);
                                break;
                            }
                        }
        
                        break;
                    }
                    foreach (var tile in _rangeFinderTiles)
                    {
                        if (tile.isBlocked)
                        {
                            continue;
                        }
                        // gets the path to the current tile and all players in melee range of that tile
                        var tempMovePath = _pathFinder.FindPath(standingOnTile, tile,
                            _rangeFinderTiles, false);
                        var playersInMeleeRange = GetPlayersInMeleeRange(tile);
                        switch (playersInMeleeRange.Count)
                        {
                            // if there are no players go to next tile
                            case 0:
                                continue;
                            // if there is one calculate the temp strategy
                            case 1:
                            {
                                var playerInfo = PlayerDict[playersInMeleeRange.First()];
                                tempStrategy.StrategyAttackValue = CalculateMeleeAttackValue(playerInfo);
                                tempStrategy.StrategyMovementValue =
                                    CalculateMovementValue(tempMovePath);
                                tempStrategy.TargetTile = tile;
                                tempStrategy.CurrentTile = standingOnTile;
                                tempStrategy.PlayerToAttack = playersInMeleeRange[0];
                                if (tempStrategy.StrategyValue >
                                    strategy.StrategyValue) // compare the temp strategy to the current strategy and replace if larger
                                {
                                    strategy = new Strategy(tempStrategy.TargetTile,
                                                tempStrategy.CurrentTile, tempStrategy.PlayerToAttack, tempStrategy.AbilityToUse, tempStrategy.StrategyAttackValue,
                                                tempStrategy.StrategyMovementValue);
                                    
                                }
                                Debug.Log(strategy);
                                break;
                            }
                            // if there are multiple players
                            case > 1:
                            {
                                var lowestPlayer =
                                    GetLowestPlayer(playersInMeleeRange); // get player with the lowest health
                                var playerInfo = PlayerDict[lowestPlayer];
                                tempStrategy.StrategyAttackValue =
                                    CalculateMeleeAttackValue(playerInfo); // calculate strategy
                                tempStrategy.StrategyMovementValue =
                                    CalculateMovementValue(tempMovePath);
                                tempStrategy.TargetTile = tile;
                                tempStrategy.CurrentTile = standingOnTile;
                                tempStrategy.PlayerToAttack = lowestPlayer;
                                if (tempStrategy.StrategyValue >
                                    strategy.StrategyValue) //compare strategy and update if larger
                                {
                                    strategy = new Strategy(tempStrategy.TargetTile,
                                                tempStrategy.CurrentTile, tempStrategy.PlayerToAttack, tempStrategy.AbilityToUse, tempStrategy.StrategyAttackValue,
                                                tempStrategy.StrategyMovementValue);
                                }
                                Debug.Log(strategy);
                                break;
                            }
                        }
                    }
        
                    // if no strategies were found for all tiles get paths to all current players in map
                    if (!strategy.TargetTile)
                    {
                        var bestPlayer = GetBestPlayer();

                        if (!PlayerDict.ContainsKey(bestPlayer))
                            Debug.LogError($"Player {bestPlayer} not found in PlayerDict.");

                        var playerInfo = PlayerDict[bestPlayer];
                        foreach (var tile in _rangeFinderTiles)
                        {
                            if (tile.isBlocked)
                            {
                                continue;
                            }
                            var distanceToPlayer = GetDistance(tile, playerInfo.standingOnTile);
                            var tempMovePath = _pathFinder.FindPath(standingOnTile, tile,
                                _rangeFinderTiles, false);
                            tempStrategy.StrategyAttackValue =
                                0f; // calculate strategy
                            tempStrategy.StrategyMovementValue =
                                CalculateMovementValue(tempMovePath) - distanceToPlayer;
                            tempStrategy.TargetTile = tile;
                            tempStrategy.CurrentTile = standingOnTile;
                            tempStrategy.PlayerToAttack = null;
                            if (tempStrategy.StrategyValue >
                                strategy.StrategyValue) //compare strategy and update if larger
                            {
                                strategy = new Strategy(tempStrategy.TargetTile,
                                                tempStrategy.CurrentTile, tempStrategy.PlayerToAttack, tempStrategy.AbilityToUse, tempStrategy.StrategyAttackValue,
                                                tempStrategy.StrategyMovementValue);
                            }
                        }
                    }
                    Debug.Log(strategy);
                    break;
                }
                // if the enemy type is Ranged
                case "Ranged":
                {
                    // create a new blank strategy
                    var tempStrategy = new Strategy();
                    // get players within the current movement range of the enemy
                    var playersInRange = GetPlayersInRange();
                    // check if there are any players within that range
                    if (playersInRange.Count > 0)
                    {
                        switch (playersInRange.Count)
                        {
                            // if only 1 player in range get tile and info for it
                            case 1:
                            {
                                var playerInfo = PlayerDict[playersInRange[0]];
                                // for each tile in movement range of the enemy
                                foreach (var tile in _rangeFinderTiles)
                                {
                                    // get the path to that tile and the distance to the player from the tile
                                    var tempMovePath = _pathFinder.FindPath(tile, standingOnTile,
                                        _rangeFinderTiles, false);
                                    var distanceToPlayer = GetDistance(tile, playerInfo.standingOnTile);
                                    // skip the current player tile
                                    if (tile.isBlocked || CheckIfTileIsPlayerTile(tile) ||IsAttackBlocked(tile, playerInfo.standingOnTile))
                                    {
                                        continue;
                                    }
                                    // get all the tiles within movement range of the destination tile, and the players/paths associated;
                                    var rangeTiles = _rangeFinder.GetTilesInRange(tile.Grid2DLocation, movement);
                                    var rangePlayers = GetPlayersInRange(rangeTiles);
                                    rangePlayers.Remove(playersInRange[0]);
                                    switch (rangePlayers.Count)
                                    {
                                        case 0:
                                        {
                                            break;
                                        }
                                        // if there is 1 player within movement range of the destination tile
                                        case 1:
                                        {
                                            // gets distance from destination tile to second range player
                                            var distanceToSecondPlayer = GetDistance(tile, PlayerDict[rangePlayers[0]].standingOnTile);
                                            // calculate a new strategy based off the current info
                                            tempStrategy.StrategyMovementValue =
                                                 CalculateRangedMovementValue(tempMovePath) + distanceToPlayer + distanceToSecondPlayer;
                                            tempStrategy.StrategyAttackValue =
                                                 CalculateRangedAttackValue(playerInfo, distanceToPlayer);
                                            tempStrategy.TargetTile = tile;
                                            tempStrategy.CurrentTile = standingOnTile;
                                            tempStrategy.PlayerToAttack = playersInRange[0];
                                            // compare to current strategy and update if better
                                            if (tempStrategy.StrategyValue > strategy.StrategyValue)
                                            {
                                                strategy = new Strategy(tempStrategy.TargetTile,
                                                tempStrategy.CurrentTile, tempStrategy.PlayerToAttack, tempStrategy.AbilityToUse, tempStrategy.StrategyAttackValue,
                                                tempStrategy.StrategyMovementValue);
                                            }
                                            break;
                                        }
                                        // if there are multiple players in movement range of the destination tile
                                        case > 1:
                                        {
                                            var combinedDist = 0;
                                            // combine the path values of all players in range and calculate a new strategy
                                            foreach (var pl in rangePlayers)
                                            {
                                                combinedDist += GetDistance(tile, PlayerDict[pl].standingOnTile);
                                            }
                                            tempStrategy.StrategyMovementValue =
                                                CalculateRangedMovementValue(tempMovePath) + distanceToPlayer + combinedDist;
                                            tempStrategy.StrategyAttackValue =
                                                CalculateRangedAttackValue(playerInfo, distanceToPlayer);
                                            tempStrategy.TargetTile = tile;
                                            tempStrategy.CurrentTile = standingOnTile;
                                            tempStrategy.PlayerToAttack = playersInRange[0];
                                            // compare and update if better
                                            if (tempStrategy.StrategyValue > strategy.StrategyValue)
                                            {
                                                strategy = new Strategy(tempStrategy.TargetTile,
                                                tempStrategy.CurrentTile, tempStrategy.PlayerToAttack, tempStrategy.AbilityToUse, tempStrategy.StrategyAttackValue,
                                                tempStrategy.StrategyMovementValue);
                                            }
                                            break;
                                        }
                                    }
                                    // calculate a new strategy based off the current info
                                    tempStrategy.StrategyMovementValue =
                                        CalculateRangedMovementValue(tempMovePath) + distanceToPlayer + distanceToPlayer;
                                    tempStrategy.StrategyAttackValue =
                                        CalculateRangedAttackValue(playerInfo, distanceToPlayer);
                                    tempStrategy.TargetTile = tile;
                                    tempStrategy.CurrentTile = standingOnTile;
                                    tempStrategy.PlayerToAttack = playersInRange[0];
                                    // compare to current strategy and update if better
                                    if (tempStrategy.StrategyValue > strategy.StrategyValue)
                                    {
                                        strategy = new Strategy(tempStrategy.TargetTile,
                                                tempStrategy.CurrentTile, tempStrategy.PlayerToAttack, tempStrategy.AbilityToUse, tempStrategy.StrategyAttackValue,
                                                tempStrategy.StrategyMovementValue);
                                    }
                                }
                                Debug.Log(strategy + "1 player in movement range");
                                break;
                            }
                            // if multiple players in movement range of current tile
                            case > 1:
                            {
                                // get the best player to attack
                                var bestPlayer = GetBestPlayer(playersInRange);
                                var playerInfo = PlayerDict[bestPlayer];
                                // check all tiles in movement range
                                foreach (var tile in _rangeFinderTiles)
                                {
                                    var tempMovePath = _pathFinder.FindPath(tile, standingOnTile,
                                        _rangeFinderTiles, false);
                                    var distanceToPlayer = GetDistance(tile, playerInfo.standingOnTile);
                                    // skip the current player tile
                                    if (tile.isBlocked || CheckIfTileIsPlayerTile(tile) || IsAttackBlocked(tile, playerInfo.standingOnTile))
                                    {
                                        continue;
                                    }
                                    // get all the tiles within movement range of the destination tile, and the players/paths associated;
                                    var rangeTiles = _rangeFinder.GetTilesInRange(tile.Grid2DLocation, movement);
                                    var rangePlayers = GetPlayersInRange(rangeTiles);
                                    rangePlayers.Remove(bestPlayer);
                                    switch (rangePlayers.Count)
                                    {
                                        case 0:
                                        {
                                            break;
                                        }
                                        case 1:
                                        {
                                            // gets distance from destination tile to second range player
                                            var distanceToSecondPlayer = GetDistance(tile, PlayerDict[rangePlayers[0]].standingOnTile);
                                            tempStrategy.StrategyMovementValue =
                                                 CalculateRangedMovementValue(tempMovePath) + distanceToPlayer + distanceToSecondPlayer;
                                            tempStrategy.StrategyAttackValue =
                                                 CalculateRangedAttackValue(playerInfo, distanceToPlayer);
                                            tempStrategy.TargetTile = tile;
                                            tempStrategy.CurrentTile = standingOnTile;
                                            tempStrategy.PlayerToAttack = bestPlayer;
                                            if (tempStrategy.StrategyValue > strategy.StrategyValue)
                                            {
                                                strategy = new Strategy(tempStrategy.TargetTile,
                                                tempStrategy.CurrentTile, tempStrategy.PlayerToAttack, tempStrategy.AbilityToUse, tempStrategy.StrategyAttackValue,
                                                tempStrategy.StrategyMovementValue);
                                            }
                                            break;
                                        }
                                        case > 1:
                                        {
                                            var combinedDist = 0;
                                            foreach (var pl in rangePlayers)
                                            {
                                                combinedDist += GetDistance(tile, PlayerDict[pl].standingOnTile);
                                            }
                                            tempStrategy.StrategyMovementValue =
                                                CalculateRangedMovementValue(tempMovePath) + distanceToPlayer + combinedDist;
                                            tempStrategy.StrategyAttackValue =
                                                CalculateRangedAttackValue(playerInfo, distanceToPlayer);
                                            tempStrategy.TargetTile = tile;
                                            tempStrategy.CurrentTile = standingOnTile;
                                            tempStrategy.PlayerToAttack = bestPlayer;
                                            if (tempStrategy.StrategyValue > strategy.StrategyValue)
                                            {
                                                strategy = new Strategy(tempStrategy.TargetTile,
                                                tempStrategy.CurrentTile, tempStrategy.PlayerToAttack, tempStrategy.AbilityToUse, tempStrategy.StrategyAttackValue,
                                                tempStrategy.StrategyMovementValue);
                                            }
                                            break;
                                        } 
                                    }
                                    tempStrategy.StrategyMovementValue =
                                        CalculateRangedMovementValue(tempMovePath) + distanceToPlayer + distanceToPlayer;
                                    tempStrategy.StrategyAttackValue =
                                        CalculateRangedAttackValue(playerInfo, distanceToPlayer);
                                    tempStrategy.TargetTile = tile;
                                    tempStrategy.CurrentTile = standingOnTile;
                                    tempStrategy.PlayerToAttack = bestPlayer;
                                    if (tempStrategy.StrategyValue > strategy.StrategyValue)
                                    {
                                        strategy = new Strategy(tempStrategy.TargetTile,
                                                tempStrategy.CurrentTile, tempStrategy.PlayerToAttack, tempStrategy.AbilityToUse, tempStrategy.StrategyAttackValue,
                                                tempStrategy.StrategyMovementValue);
                                    }
                                }
                                Debug.Log(strategy + "multiple players in movement range");
                                break;
                            }
                        }
        
                        return strategy;
                    }
                    foreach (var tile in _rangeFinderTiles)
                    {
                        var tempMovePath = _pathFinder.FindPath(standingOnTile, tile,
                            _rangeFinderTiles, false);
                        var playersInRangedRange = GetPlayersInRangedRange(tile);
                        if (abilities.Count > 0)
                        {
                            foreach (var ability in abilities)
                            {
                                if (ability.manaCost > mana)
                                    continue;
                                var tilesInAttackRange = GetInRangeTiles(tile, ability.attackRange);
                                foreach (var t in tilesInAttackRange)
                                {
                                    if (ability.areaOfEffect > 0)
                                    {
                                        var areaOfEffectTiles = GetInRangeTiles(t, ability.areaOfEffect);
                                        var alliesAffected = GetAlliesAffected(areaOfEffectTiles);
                                        var playersInAreaOfEffect = GetPlayersInRange(areaOfEffectTiles);
                                                
                                        switch (playersInAreaOfEffect.Count)
                                        {
                                            case 0:
                                                continue;
                                            // if there is only one in range
                                            case 1:
                                            {
                                                var playerInfo = PlayerDict[playersInAreaOfEffect[0]];
                                                if (tile.isBlocked || CheckIfTileIsPlayerTile(tile) || IsAttackBlocked(tile, playerInfo.standingOnTile))
                                                {
                                                    continue;
                                                }
                                                tempStrategy.StrategyAttackValue =
                                                    CalculateAbilityAttackValue(playerInfo, ability,
                                                        playersInAreaOfEffect, alliesAffected);
                                                tempStrategy.StrategyMovementValue =
                                                    CalculateRangedMovementValue(tempMovePath);
                                                tempStrategy.TargetTile = tile;
                                                tempStrategy.CurrentTile = standingOnTile;
                                                tempStrategy.PlayerToAttack = playersInAreaOfEffect[0];
                                                tempStrategy.AbilityToUse = ability;
                                                if (tempStrategy.StrategyValue > strategy.StrategyValue)
                                                {
                                                    strategy = new Strategy(tempStrategy.TargetTile,
                                                tempStrategy.CurrentTile, tempStrategy.PlayerToAttack, tempStrategy.AbilityToUse, tempStrategy.StrategyAttackValue,
                                                tempStrategy.StrategyMovementValue);
                                                }
                                                Debug.Log(strategy + "1 players in ability range");
                                                break;
                                            }
                                            // if there are multiple enemies in range
                                            case > 1:
                                            {
                                                foreach (var p in playersInAreaOfEffect)
                                                {
                                                    var playerInfo = PlayerDict[p];
                                                    if (tile.isBlocked || CheckIfTileIsPlayerTile(tile) || IsAttackBlocked(tile, playerInfo.standingOnTile))
                                                    {
                                                        continue;
                                                    }
                                                    tempStrategy.StrategyAttackValue =
                                                        CalculateAbilityAttackValue(playerInfo, ability, playersInAreaOfEffect,
                                                            alliesAffected);
                                                    tempStrategy.StrategyMovementValue =
                                                        CalculateRangedMovementValue(tempMovePath);
                                                    tempStrategy.TargetTile = tile;
                                                    tempStrategy.CurrentTile = standingOnTile;
                                                    tempStrategy.PlayerToAttack = p;
                                                    tempStrategy.AbilityToUse = ability;
                                                    if (tempStrategy.StrategyValue > strategy.StrategyValue)
                                                    {
                                                        strategy = new Strategy(tempStrategy.TargetTile,
                                                tempStrategy.CurrentTile, tempStrategy.PlayerToAttack, tempStrategy.AbilityToUse, tempStrategy.StrategyAttackValue,
                                                tempStrategy.StrategyMovementValue);
                                                    }
                                                }
                                                Debug.Log(strategy + "multiple players in ability range");
                                                break;
                                            }
                                        }
                                        continue;
                                    }
                                    var playersInAbilityRange = GetPlayersInRange(tilesInAttackRange);
                                    switch (playersInAbilityRange.Count)
                                    {
                                        case 0:
                                            continue;
                                        // if there is only one in range
                                        case 1:
                                        {
                                            var playerInfo = PlayerDict[playersInAbilityRange[0]];
                                            if (tile.isBlocked || CheckIfTileIsPlayerTile(tile) || IsAttackBlocked(tile, playerInfo.standingOnTile))
                                            {
                                                continue;
                                            }
                                            tempStrategy.StrategyAttackValue =
                                                CalculateAbilityAttackValue(playerInfo, ability,
                                                    playersInAbilityRange, new List<GameObject>());
                                            tempStrategy.StrategyMovementValue =
                                                CalculateRangedMovementValue(tempMovePath);
                                            tempStrategy.TargetTile = tile;
                                            tempStrategy.CurrentTile = standingOnTile;
                                            tempStrategy.PlayerToAttack = playersInAbilityRange[0];
                                            tempStrategy.AbilityToUse = ability;
                                            if (tempStrategy.StrategyValue > strategy.StrategyValue)
                                            {
                                                strategy = new Strategy(tempStrategy.TargetTile,
                                                tempStrategy.CurrentTile, tempStrategy.PlayerToAttack, tempStrategy.AbilityToUse, tempStrategy.StrategyAttackValue,
                                                tempStrategy.StrategyMovementValue);
                                            }

                                            Debug.Log(strategy + "1 players in ability range");
                                            break;
                                        }
                                        // if there are multiple enemies in range
                                        case > 1:
                                        {
                                            var bestPlayer = GetBestPlayer(playersInAbilityRange);
                                            var playerInfo = PlayerDict[bestPlayer];
                                            if (tile.isBlocked || CheckIfTileIsPlayerTile(tile) || IsAttackBlocked(tile, playerInfo.standingOnTile))
                                            {
                                                continue;
                                            }
                                            tempStrategy.StrategyAttackValue =
                                                CalculateAbilityAttackValue(playerInfo, ability,
                                                    playersInAbilityRange,
                                                    new List<GameObject>());
                                            tempStrategy.StrategyMovementValue =
                                                CalculateRangedMovementValue(tempMovePath);
                                            tempStrategy.TargetTile = tile;
                                            tempStrategy.CurrentTile = standingOnTile;
                                            tempStrategy.PlayerToAttack = bestPlayer;
                                            tempStrategy.AbilityToUse = ability;
                                            if (tempStrategy.StrategyValue > strategy.StrategyValue)
                                            {
                                                strategy = new Strategy(tempStrategy.TargetTile,
                                                tempStrategy.CurrentTile, tempStrategy.PlayerToAttack, tempStrategy.AbilityToUse, tempStrategy.StrategyAttackValue,
                                                tempStrategy.StrategyMovementValue);
                                            }
                                            Debug.Log(strategy + "multiple players in ability range");
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        switch (playersInRangedRange.Count)
                        {
                            case 0:
                            {
                                continue;
                            }
                            case 1:
                            {
                                var playerInfo = PlayerDict[playersInRangedRange[0]];
                                if (tile.isBlocked || CheckIfTileIsPlayerTile(tile) || IsAttackBlocked(tile, playerInfo.standingOnTile))
                                {
                                    continue;
                                }
                                var distanceToPlayer = GetDistance(tile, playerInfo.standingOnTile);
                                // gets distance from destination tile to second range player
                                // calculate a new strategy based off the current info
                                tempStrategy.StrategyMovementValue =
                                     CalculateRangedMovementValue(tempMovePath) + distanceToPlayer;
                                tempStrategy.StrategyAttackValue =
                                     CalculateRangedAttackValue(playerInfo, distanceToPlayer);
                                tempStrategy.TargetTile = tile;
                                tempStrategy.CurrentTile = standingOnTile;
                                tempStrategy.PlayerToAttack = playersInRangedRange[0];
                                // compare to current strategy and update if better
                                if (tempStrategy.StrategyValue > strategy.StrategyValue)
                                {
                                    strategy = new Strategy(tempStrategy.TargetTile,
                                                tempStrategy.CurrentTile, tempStrategy.PlayerToAttack, tempStrategy.AbilityToUse, tempStrategy.StrategyAttackValue,
                                                tempStrategy.StrategyMovementValue);
                                }
                                break;
                            }
                            case > 1:
                            {
                                var bestPlayer = GetBestPlayer(playersInRangedRange);
                                var playerInfo = PlayerDict[bestPlayer];
                                if (tile.isBlocked || CheckIfTileIsPlayerTile(tile) || IsAttackBlocked(tile, playerInfo.standingOnTile))
                                {
                                    continue;
                                }
                                playersInRangedRange.Remove(bestPlayer);
                                var distanceToPlayer = GetDistance(tile, playerInfo.standingOnTile);
                                // if there are multiple players in movement range of the destination tile
                           
                                var combinedDist = 0;
                                // combine the path values of all players in range and calculate a new strategy
                                foreach (var pl in playersInRangedRange)
                                {
                                    combinedDist += GetDistance(tile, PlayerDict[pl].standingOnTile);
                                }
                                tempStrategy.StrategyMovementValue =
                                    CalculateRangedMovementValue(tempMovePath) + distanceToPlayer + combinedDist;
                                tempStrategy.StrategyAttackValue =
                                    CalculateRangedAttackValue(playerInfo, distanceToPlayer);
                                tempStrategy.TargetTile = tile;
                                tempStrategy.CurrentTile = standingOnTile;
                                tempStrategy.PlayerToAttack = bestPlayer;
                                // compare and update if better
                                if (tempStrategy.StrategyValue > strategy.StrategyValue)
                                {
                                    strategy = new Strategy(tempStrategy.TargetTile,
                                                tempStrategy.CurrentTile, tempStrategy.PlayerToAttack, tempStrategy.AbilityToUse, tempStrategy.StrategyAttackValue,
                                                tempStrategy.StrategyMovementValue);
                                }
                                Debug.Log(strategy + "1 player in attack range");
                                break;
                            }
                        }
                    }
                    if (!strategy.TargetTile)
                    {
                        var bestPlayer = GetBestPlayer();
                        var playerInfo = PlayerDict[bestPlayer];
                        foreach (var tile in _rangeFinderTiles)
                        {
                            
                            var distanceToPlayer = GetDistance(tile, playerInfo.standingOnTile);
                            var tempMovePath = _pathFinder.FindPath(standingOnTile, tile,
                                _rangeFinderTiles, false);
                            if (tile.isBlocked)
                            {
                                continue;
                            }
                            var countOverRange = distanceToPlayer - weapon.weaponRange;
                            tempStrategy.StrategyAttackValue =
                                0f; // calculate strategy
                            tempStrategy.StrategyMovementValue =
                                CalculateRangedMovementValue(tempMovePath) - countOverRange;
                            tempStrategy.TargetTile = tile;
                            tempStrategy.CurrentTile = standingOnTile;
                            tempStrategy.PlayerToAttack = null;
                            if (tempStrategy.StrategyValue >
                                strategy.StrategyValue) //compare strategy and update if larger
                            {
                                strategy = new Strategy(tempStrategy.TargetTile,
                                                tempStrategy.CurrentTile, tempStrategy.PlayerToAttack, tempStrategy.AbilityToUse, tempStrategy.StrategyAttackValue,
                                                tempStrategy.StrategyMovementValue);
                            }

                        }
                    }
                    Debug.Log(strategy + "no players in range");
                    break;
                }
            }
            return strategy;
        }

        private bool IsAttackBlocked(OverlayTile start, OverlayTile end)
        {
            var attackLine = new Ray(start.gridLocation, (end.gridLocation - start.gridLocation));
            var distanceToPlayer = GetDistance(start, end);
            var attackLineColliders = new List<Collider2D>();
            var attackLineHits =  Physics2D.GetRayIntersectionAll(attackLine, distanceToPlayer).ToList();
            foreach (var hit in attackLineHits)
            {
                attackLineColliders.Add(hit.collider);   
            }
            var attackLineTiles = (from overlayTile in MapManager.Instance.Map.Values from col 
                in attackLineColliders where overlayTile.GetComponent<Collider2D>() == col select overlayTile).ToList();
            attackLineTiles.Remove(end);
            attackLineTiles.Remove(start);
            foreach (var tile in attackLineTiles)
            {
                if (tile.isBlocked)
                    return true;
            }
            return false;
        }
        
        private int GetDistance(OverlayTile start, OverlayTile end)
        {
            var distance = Vector2.Distance(start.Grid2DLocation, end.Grid2DLocation);
            return (int)distance;
        }

        private List<List<OverlayTile>> GetPlayerPaths()
        {
            return PlayerDict.Keys.Select(p => PlayerDict[p]).Select(playerInfo => 
                _pathFinder.FindPath(standingOnTile, playerInfo.standingOnTile, 
                    new List<OverlayTile>(), false)).ToList();
        }
        
        private List<List<OverlayTile>> GetPlayerPaths(List<GameObject> playersInRange)
        {
            return playersInRange.Select(p => PlayerDict[p]).Select(playerInfo =>
                _pathFinder.FindPath(standingOnTile, playerInfo.standingOnTile, 
                    new List<OverlayTile>(), false)).ToList();
        }
        
        private List<GameObject> GetPlayersInRangedRange(OverlayTile tile)
        {
            var playersInRange = new List<GameObject>();
            var inRangeTiles = GetInRangeTiles(tile, weapon.weaponRange);
            foreach (var t in inRangeTiles)
            {
                if (!CheckIfTileIsPlayerTile(t))
                {
                    continue;
                }
                playersInRange.Add(t.GetPlayerOnTile());
            }

            return playersInRange;
        }
        
        private GameObject GetLowestPlayer(List<GameObject> playersInMeleeRange)
        {
            var lowestPlayer = playersInMeleeRange[0];
            var lowest = 5000;
            foreach (var p in playersInMeleeRange)
            {
                var info = PlayerDict[p];
                if (info.health >= lowest) continue;
                lowest = info.health;
                lowestPlayer = p;
            }
            return lowestPlayer;
        }
        
        private List<GameObject> GetClosestPlayers(List<GameObject> playersInRange, GameObject bestPlayer)
        {
            if (playersInRange.Contains(bestPlayer))
            {
                playersInRange.Remove(bestPlayer);
            }
            var playerPaths = GetPlayerPaths(playersInRange);
            playerPaths.Sort((a,b) => a.Count - b.Count);
            var previousPath = new List<OverlayTile>(200);
            var closestPlayers = new List<GameObject>();
            foreach (var path in playerPaths) // find the shortest paths
            {
                if (path.Count !< previousPath.Count)
                {
                    continue;
                }
                closestPlayers.Add(path.Last().GetPlayerOnTile());
                previousPath = path;
            }

            return closestPlayers;
        }
        
        private GameObject GetBestPlayer()
        {
            var playerPaths = GetPlayerPaths();
            playerPaths.Sort((a,b) => a.Count - b.Count);
            var bestPath = playerPaths.First();

            var bestPlayer = bestPath.Last().GetPlayerOnTile();
            
            return bestPlayer;
        }
        
        private GameObject GetBestPlayer(List<GameObject> playersInRange)
        {
            var playerPaths = GetPlayerPaths(playersInRange);
            playerPaths.Sort((a,b) => a.Count - b.Count);
            var bestPath = playerPaths[0];
            playerPaths.Remove(bestPath);
            var sameCountPaths = new List<List<OverlayTile>> { bestPath };
            
            foreach (var path in playerPaths)
            {
                if (path.Count == bestPath.Count)
                {
                    sameCountPaths.Add(path);
                }
            }
            if (sameCountPaths.Count > 1)
            {
                var playerList = new List<GameObject>();
                foreach (var path in sameCountPaths)
                {
                    playerList.Add(path.Last().GetPlayerOnTile());
                }
                var lowestPlayer = GetLowestPlayer(playerList);
                return lowestPlayer;
            }
            
            var bestPlayer = bestPath.Last().GetPlayerOnTile();
            return bestPlayer;
        }
        
        private List<GameObject> GetAlliesAffected(List<OverlayTile> areaOfEffectTiles)
        {
            return (from ally in _allies let activeTile = ally.GetComponent<EnemyController>().standingOnTile
                where areaOfEffectTiles.Contains(activeTile) select ally).ToList();
        }
        
        private float CalculateMovementValue(List<OverlayTile> tempMovePath)
        {
            var playersAlongPath = new List<GameObject>();
            foreach (var p in from tile in tempMovePath select _pathFinder.GetNeightbourOverlayTiles(tile)
                     into inMeleeRange from t in inMeleeRange from p in PlayerDict.Keys 
                     let info = PlayerDict[p] where info.standingOnTile == t && 
                                                                      playersAlongPath.Contains(p) select p)
            {
                playersAlongPath.Add(p);
            }
            return InitialMovementValue - playersAlongPath.Count;
        }
        
        private float CalculateRangedMovementValue(List<OverlayTile> tempMovePath)
        {
            var newTile = standingOnTile;
            if (tempMovePath.Count > 0)
            {
                newTile = tempMovePath.Last();
            }
            var tilesInRange = _rangeFinder.GetTilesInRange(newTile.Grid2DLocation, movement);
            var playersInRange = GetPlayersInRange(tilesInRange);
            var value = 0f;
            switch (playersInRange.Count)
            {
                case 0:
                {
                    value = InitialMovementValue;
                    break;
                }
                case 1:
                {
                    var playerPaths = GetPlayerPaths(playersInRange);
                    value = InitialMovementValue + playerPaths[0].Count;
                    break;
                }
                case > 1:
                {
                    var playerPaths = GetPlayerPaths(playersInRange);
                    var combinedPathValue = playerPaths.Sum(path => path.Count);
                    value = InitialMovementValue + combinedPathValue;
                    break;
                }
            }
            
            return value;
        }
        
        private List<GameObject> GetPlayersInRange()
        {
            return (from p in PlayerDict.Keys let playerInfo = PlayerDict[p] from tile in _rangeFinderTiles
                where tile == playerInfo.standingOnTile select p).ToList();
        }
        
        private List<GameObject> GetPlayersInRange(List<OverlayTile> rangeTiles)
        {
            return (from p in PlayerDict.Keys let playerInfo = PlayerDict[p] from tile in rangeTiles
                where tile == playerInfo.standingOnTile select p).ToList();
        }
        
        private float CalculateMeleeAttackValue(CharacterInfo playerInfo)
        {
            var hitValue = 0f;
            var dmgDifference = 0f; 
            var playerHealth = playerInfo.health;
            if ((weapon.averageDmg + _statBonuses[weapon.weaponStat]) !>
                playerHealth)
            {
                hitValue = weapon.averageDmg +
                            _statBonuses[weapon.weaponStat];
                dmgDifference = playerHealth - hitValue;
                return hitValue + dmgDifference;
            }
            hitValue = weapon.averageDmg +
                       _statBonuses[weapon.weaponStat];
            dmgDifference = hitValue - playerHealth;
            return  hitValue + dmgDifference + 100f;
        }
        
        private float CalculateRangedAttackValue(CharacterInfo playerInfo, int distanceToPlayer)
        {
            if (distanceToPlayer < 2 && weapon.averageDmg - playerInfo.health > 0)
            {
                return -10000;
            }
            var attackValue = distanceToPlayer;
            return attackValue;
        }
        
        private List<GameObject> GetPlayersInMeleeRange(OverlayTile tile)
        {
            var inRangeTiles = GetInRangeTiles(tile, weapon.weaponRange);
            return GetPlayersInRange(inRangeTiles);
        }
        
        private float CalculateAbilityAttackValue(CharacterInfo playerInfo, Ability ability,
            List<GameObject> playersInAbilityRange, List<GameObject> alliesAffected)
        {
            float hitValue;
            var playersAffected = playersInAbilityRange.Count;
            if ((ability.averageDmg + _statBonuses[ability.stat]) !> playerInfo.health && alliesAffected.Count == 0)
            {
                hitValue = (ability.averageDmg + _statBonuses[ability.stat]) - playerInfo.health;
                var dmgDifference = Math.Abs(hitValue - playerInfo.health);
                return hitValue + playersAffected + dmgDifference;
            }
            if ((ability.averageDmg + _statBonuses[ability.stat]) !> playerInfo.health && alliesAffected.Count > 0)
            {
                hitValue = (ability.averageDmg + _statBonuses[ability.stat]) - playerInfo.health;
                var dmgDifference = Math.Abs(hitValue - playerInfo.health);
                return (hitValue + playersAffected + dmgDifference) * .25f;
            }
            if ((ability.averageDmg + _statBonuses[ability.stat]) > playerInfo.health && alliesAffected.Count == 0)
            {
                hitValue = (ability.averageDmg + _statBonuses[ability.stat]) - playerInfo.health;
                var dmgDifference = Math.Abs(hitValue - playerInfo.health);
                return hitValue + playersAffected + dmgDifference + 100f;
            }
            if ((ability.averageDmg + _statBonuses[ability.stat]) > playerInfo.health && alliesAffected.Count > 0)
            {
                hitValue = (ability.averageDmg + _statBonuses[ability.stat]) - playerInfo.health;
                var dmgDifference = Math.Abs(hitValue - playerInfo.health);
                return (hitValue + playersAffected + dmgDifference + 100f) * .25f;
            }
        
            return 0f;
        }

        private void AttackPlayer(GameObject playerToAttack)
        {
            if (playerToAttack)
            {
                switch (enemyType)
                {
                    case "Melee":
                    {
                        var playersInMeleeRange = GetPlayersInMeleeRange(standingOnTile);
                        if (playersInMeleeRange.Contains(playerToAttack))
                        {
                            _combatController.AttackOtherCharacter(this, PlayerDict[playerToAttack]);//TODO play animation
                            hasAttack = false;
                            break;
                        }
    
                        hasAttack = false;
                        break;
                    }
                    case "Ranged":
                    {
                        if (playerToAttack)
                        {
                            _combatController.AttackOtherCharacter(this, PlayerDict[playerToAttack]);
                            
                            hasAttack = false;
                            break;
                        }
    
                        hasAttack = false;
                        break;
                    }
                }
            }
            hasAttack = false;

        }
    }
}

