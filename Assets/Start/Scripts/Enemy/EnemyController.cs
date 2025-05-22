using System;
using System.Collections.Generic;
using System.Linq;
using Start.Scripts.Game;
using Start.Scripts.Inventory;
using UnityEngine;
using Random = System.Random;

namespace Start.Scripts.Enemy
{
    public class EnemyController : MonoBehaviour, IGameManagerAware
    {
        [SerializeField] private float speed;
        [SerializeField] public EnemyData data;
        [SerializeField] private int movement;
        [SerializeField] public int initiative;
        [SerializeField] private GameObject playerContainer;
        [SerializeField] private GameObject enemyContainer;
        [SerializeField] public OverlayTile standingOnTile;
        [SerializeField] public int health;
        [SerializeField] public int maxHealth;
        [SerializeField] public int mana;
        [SerializeField] public int maxMana;
        [SerializeField] public List<Ability> abilities;
        [SerializeField] public List<string> statType;
        [SerializeField] public List<int> stats;
        [SerializeField] public int bonusToHit;
        [SerializeField] public int armorClass;
        [SerializeField] public InventoryItemData weapon;
        [SerializeField] public InventoryItemData armor;
        [SerializeField] public bool hasDisadvantage;
        [SerializeField] public bool hasAdvantage;
        [SerializeField] public bool hasMovement;
        [SerializeField] public bool hasAttack;
        [SerializeField] public string enemyType;

        private const float InitialMovementValue = 20f;
        private List<GameObject> _allies;
        private CombatController _combatController;
        private PathFinder _pathFinder;
        private RangeFinder _rangeFinder;
        private List<OverlayTile> _path;

        private List<OverlayTile> _rangeFinderTiles;    
        private List<OverlayTile> _rangeTileDistances;
        public Dictionary<GameObject, CharacterInfo> PlayerDict;
        private Dictionary<string, int> _statBonuses;
        private int _spellSlotsAvailable;
        private Strategy _strategy;
        private bool _isMoving;
        private bool _strategyFound;

        // GameManager integration
        private GameManager _gameManager;
        private IAIStrategy _aiStrategy;

        // runs when the object becomes awake
        private void Awake()
        {
            _combatController = GetComponent<CombatController>();
            _pathFinder = new PathFinder();
            _rangeFinder = new RangeFinder();
            _path = new List<OverlayTile>();
            _rangeFinderTiles = new List<OverlayTile>();
            _statBonuses = new Dictionary<string, int>();
            _isMoving = false;
            _strategyFound = false;
        }

        // Start is called before the first frame update
        private void Start()
        {
            enemyContainer = GameObject.FindGameObjectWithTag("Enemies");
            playerContainer = GameObject.FindGameObjectWithTag("Players").gameObject;
            PlayerDict = new Dictionary<GameObject, CharacterInfo>();
            SetEnemyValues();
            _strategy = new Strategy();
            _allies = new List<GameObject>();
            _pathFinder = new PathFinder();
            _rangeFinder = new RangeFinder();
            _path = new List<OverlayTile>();
            _isMoving = false;
            _rangeFinderTiles = new List<OverlayTile>();
            
        }

        // Update is called once per frame
        void Update()
        {
            if (PlayerDict.Count == 0)
                GetPlayers();
            if (_allies.Count == 0)
                GetAllies();
            // checks if it is the enemy's turn.
            if (_combatController.isTurn)
            {
                UpdatePlayers();
                GetInRangeTiles();
                // gets player object, range, and tile.
                if (!_strategyFound)
                {
                    _strategy = _aiStrategy.EvaluateStrategy(this, _gameManager);
                    _strategyFound = true;
                }
                if (hasMovement && _strategyFound)
                {
                    GetInRangeTiles();
                    // if player is not in range and enemy is not moving
                    if (!_isMoving)
                    {
                        // path covers the entire map
                        _path = _pathFinder.FindPath(_strategy.CurrentTile,
                            _strategy.TargetTile, new List<OverlayTile>(), false);
                    }
                    MoveCharacter(_strategy.TargetTile);
                }
                
                if (hasAttack && !hasMovement)
                {
                    AttackPlayer(_strategy.PlayerToAttack);
                }
                if (!hasMovement && !hasAttack)
                {
                    EndTurn();
                }
            }
            
        }
        
        // private enum EnemyType
        // {
        //     Melee,
        //     Ranged,
        //     Versatile,
        //     Magic,
        //     Rounded,
        //     Boss
        // }
        
        // get current players in game
        private void GetPlayers()
        {
            if (playerContainer.transform.childCount == 0) return;
            for(var i = 0; i < playerContainer.transform.childCount; i++)
            {
                var player = playerContainer.transform.GetChild(i).gameObject;
                if(!PlayerDict.Keys.Contains(player))
                    PlayerDict.Add(player, player.GetComponent<CharacterInfo>());
            }
        }

        private void UpdatePlayers()
        {
            PlayerDict.Clear();
            for (var i = 0; i < playerContainer.transform.childCount; i++)
            {
                var player = playerContainer.transform.GetChild(i).gameObject;
                if(!PlayerDict.Keys.Contains(player))
                    PlayerDict.Add(player, player.GetComponent<CharacterInfo>());
            }
        }

        private void GetAllies()
        {
            for (var i = 0; i < enemyContainer.transform.childCount; i++)
            {
                var enemy = enemyContainer.transform.GetChild(i);
                if (enemy.gameObject != gameObject)
                    _allies.Add(enemy.gameObject);
            }
        }

        private bool CheckIfTileIsPlayerTile(OverlayTile tile)
        {
            return PlayerDict.Values.Any(p => p.standingOnTile == tile);
        }
        
        // determine what action is best for the enemy to make.
        private Strategy DetermineBestStrategy()
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

        /** MoveCharacter()
         * description: sets the player to moving status and makes sure the path is of correct
         *              length
         * @param Tile = the current tile to move to.
         * @return void
         */
        private void MoveCharacter(OverlayTile tile)
        {
            // checks if enemy exists and that the tile is not the enemies current tile
            if (gameObject && tile != standingOnTile)
            {
                _isMoving = true;
            }

            // Checks to see if path has more tiles, the enemy is moving, and the enemy
            // is still within the correct movement range.  ends turn if not.
            if (_path.Count > 0 && _isMoving)
            {
                MoveAlongPath();
            }
            else
            {
                ResetTiles();
                GetInRangeTiles();
                _isMoving = false;
                hasMovement = false;
            }
        }

        private void EndTurn()
        {
            _strategyFound = false;
            _combatController.StopTurn();
        }

        /** MoveAlongPath()
         * description: gets the enemies speed of movement and then moves the player
         * @return void
         */
        private void MoveAlongPath()
        {
            // slows movement for the visual
            var step = speed * Time.deltaTime;

            var zIndex = _path[0].transform.position.z;
            
            // moves enemy towards the current path tile
            transform.position = 
                Vector2.MoveTowards(transform.position, 
                    _path[0].transform.position, step);
            transform.position = new Vector3(transform.position.x,
                transform.position.y, zIndex);
            
            // checks the distance between the enemy and the path tile and updates if the
            // distance is small enough.
            if (Vector2.Distance(transform.position, _path[0].transform.position) 
                < 0.00001f)
            {
                PositionEnemyOnLine(_path[0]);
                _path.RemoveAt(0);
            }


            if (_path.Count != 0 || !gameObject) return;
            ResetTiles();
            GetInRangeTiles();
            hasMovement = false;
            _isMoving = false;
        }
        
        /** ResetTiles()
         * description: resets all tiles not in use back to original color and sprite.
         * @return void
         */
        private void ResetTiles()
        {
            // checks all tiles in map
            foreach (var overlayTile in MapManager.Instance.Map.Values)
            {
                if (!_rangeFinderTiles.Contains(overlayTile) && !_path.Contains(overlayTile) 
                                                             && overlayTile != standingOnTile)
                {
                    overlayTile.HideTile();
                }

                if (!_path.Contains(overlayTile))
                    overlayTile.ResetSprite();
            }
        }
        
        /** PositionEnemyOnLine()
         * description: places enemy on the tile specified and updates its characterInfo.
         * @param tile = tile to place the enemy on.
         * @return void
         */
        private void PositionEnemyOnLine(OverlayTile tile)
        {
            var tilePos = tile.transform.position;
            transform.position = new Vector3(tilePos.x, tilePos.y + 0.0001f, tilePos.z);
            standingOnTile = tile;
        }
        
        /** GetInRangeTiles()
         * description: gets all tiles within the enemies current movement range and
         *              saves them to a variable.
         * @return void
         */
        private void GetInRangeTiles()
        {
            _rangeFinderTiles = _rangeFinder.GetTilesInRange(
                new Vector2Int(standingOnTile.gridLocation.x, 
                    standingOnTile.gridLocation.y), movement);
      
        }
        
        /** @override GetInRangeTiles()
         * description: gets all tiles within the enemies current movement range and
         *              saves them to a variable.
         * @param tile: starting tile
         * @param characterMovement: range the character can move
         * @return: tilesInRange
         */
        private List<OverlayTile> GetInRangeTiles(OverlayTile tile, int range)
        {
            var tilesInRange = _rangeFinder.GetTilesInRange(tile.Grid2DLocation, range);
            return tilesInRange;
        }

        /** SetEnemyValues()
         * description: sets the initial values of the enemy.
         * @return void
         */
        private void SetEnemyValues()
        {
            enemyType = data.enemyType;
            movement = data.movement;
            speed = data.speed;
            health = data.health;
            maxHealth = data.maxHealth;
            mana = data.mana;
            maxMana = data.maxMana;
            hasAttack = data.hasAttack;
            hasDisadvantage = data.hasDisadvantage;
            hasMovement = data.hasMovement;
            hasAdvantage = data.hasAdvantage;
            abilities = data.abilities;
            statType = data.statType;
            stats = data.stats;
            weapon = data.weapon;
            armor = data.armor;
            CalculateStatBonuses();
            RollInitiative();
        }

        private void CalculateStatBonuses()
        {
            for (var i = 0; i < stats.Count; i++)
            {
                var stat = stats[i];
                if (stat == 1)
                {
                    _statBonuses.Add(statType[i], -5);
                }if (stat is >= 2 and <= 3)
                {
                    _statBonuses.Add(statType[i], -4);
                }if (stat is >= 4 and <= 5)
                {
                    _statBonuses.Add(statType[i], -3);
                }if (stat is >= 6 and <= 7)
                {
                    _statBonuses.Add(statType[i], -2);
                }if (stat is >= 8 and <= 9)
                {
                    _statBonuses.Add(statType[i], -1);
                }if (stat is >= 10 and <= 11)
                {
                    _statBonuses.Add(statType[i], 0);
                }if (stat is >= 12 and <= 13)
                {
                    _statBonuses.Add(statType[i], 1);
                }if (stat is >= 14 and <= 15)
                {
                    _statBonuses.Add(statType[i], 2);
                }if (stat is >= 16 and <= 17)
                {
                    _statBonuses.Add(statType[i], 3);
                }if (stat is >= 18 and <= 19)
                {
                    _statBonuses.Add(statType[i], 4);
                }if (stat is >= 20 and <= 21)
                {
                    _statBonuses.Add(statType[i], 5);
                }if (stat is >= 22 and <= 23)
                {
                    _statBonuses.Add(statType[i], 6);
                }if (stat is >= 24 and <= 25)
                {
                    _statBonuses.Add(statType[i], 7);
                }if (stat is >= 26 and <= 27)
                {
                    _statBonuses.Add(statType[i], 8);
                }if (stat is >= 28 and <= 29)
                {
                    _statBonuses.Add(statType[i], 9);
                }if (stat == 30)
                {
                    _statBonuses.Add(statType[i], 10);
                }

            }
        }
        
        /** RollInitiative()
         * description: gets the initiative of the current enemy.
         * @return void
         */
        private void RollInitiative()
        {
            var rand = new Random();
            initiative = rand.Next(1, 20);
        }

        public void Initialize(GameManager gameManager)
        {
            _gameManager = gameManager;
            
            // Subscribe to events
            _gameManager.OnCombatStarted += OnCombatStarted;
            _gameManager.OnCombatEnded += OnCombatEnded;
            
            // Set up AI strategy based on enemy type
            _aiStrategy = AIStrategyFactory.GetStrategy(enemyType);
            
            // Initialize from data if available
            if (data != null)
            {
                SetEnemyValues();
            }
        }
        
        public void OnGameStateChanged(GameManager.GameState newState)
        {
            // Handle game state changes if needed
            switch (newState)
            {
                case GameManager.GameState.Combat:
                    // Reset state for new combat
                    _strategyFound = false;
                    break;
                
                case GameManager.GameState.Exploration:
                    // Reset movement and attack flags
                    hasMovement = true;
                    hasAttack = true;
                    break;
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks
            if (_gameManager != null)
            {
                _gameManager.OnCombatStarted -= OnCombatStarted;
                _gameManager.OnCombatEnded -= OnCombatEnded;
            }
        }
        
        private void OnCombatStarted()
        {
            // Reset for combat
            hasMovement = true;
            hasAttack = true;
            _strategyFound = false;
        }
        
        private void OnCombatEnded()
        {
            // Reset after combat
            _path.Clear();
            _isMoving = false;
            _strategyFound = false;
        }
        
        // Provides access to stat bonuses for strategies
        public int GetStatBonus(string statName)
        {
            return _statBonuses.ContainsKey(statName) ? _statBonuses[statName] : 0;
        }
        
        // Utility method to get players in range for strategies
        public List<CharacterInfo> GetPartyMembersInRange(OverlayTile tile, int range)
        {
            var tilesInRange = _rangeFinder.GetTilesInRange(tile.Grid2DLocation, range);
            var players = new List<CharacterInfo>();
            
            foreach (var t in tilesInRange)
            {
                if (CheckIfTileIsPlayerTile(t))
                {
                    var player = t.GetPlayerOnTile();
                    if (player && PlayerDict.ContainsKey(player))
                    {
                        players.Add(PlayerDict[player]);
                    }
                }
            }
            
            return players;
        }
    }
}
