using System;
using System.Collections.Generic;
using System.Linq;
using Start.Scripts.Game;
using Start.Scripts.Map;
using Start.Scripts.Enemy.Strategies;
using UnityEngine;
using Start.Scripts.Combat;
using Start.Scripts.Character;
using Start.Scripts.Serialization;
using Start.Scripts.BaseClasses;
using Start.Scripts.Interfaces;

namespace Start.Scripts.Enemy
{
    public class EnemyController : Actor
    {
        private IReadOnlyList<GameObject> _allies;
        private List<OverlayTile> _rangeFinderTiles;
        private List<OverlayTile> _rangeTileDistances;
        private Strategy _strategy;
        private bool _strategyFound;
        private IAIStrategy _aiStrategy;
        private EnemyData _enemyData;
        [SerializeField] private GameObject playerContainer;
        [SerializeField] private GameObject enemyContainer;

        public EnemyData EnemyData => _enemyData;

        public event Action OnEnemyLoaded;

        // runs when the object becomes awake
        private void Awake()
        {
            _combatController = GetComponent<CombatController>();
        }

        // Start is called before the first frame update
        protected override void Start()
        {
            _strategy = new Strategy();
            _allies = _gameManager.Enemies.EnemyObjects;
            _path = new List<OverlayTile>();
            _isMoving = false;
            _rangeFinderTiles = new List<OverlayTile>();
        }

        // Update is called once per frame
        void Update()
        {
            // checks if it is the enemy's turn.
            if (_isTurn)
            {
                // gets player object, range, and tile.
                if (!_strategyFound)
                {
                    _strategy = _aiStrategy.EvaluateStrategy(this);
                    _strategyFound = true;
                }
                if (EnemyData.HasMovement && _strategyFound)
                {
                    // if player is not in range and enemy is not moving
                    if (!_isMoving)
                    {
                        // path covers the entire map
                        _path = _gameManager.PathFinder.FindPath(_strategy.CurrentTile,
                            _strategy.TargetTile, new List<OverlayTile>(), false);
                    }
                    MoveCharacter(_strategy.TargetTile);
                }
                if (EnemyData.HasAction && !EnemyData.HasMovement)
                {
                    AttackPlayer(_strategy.PlayerToAttack);
                }
                if (!EnemyData.HasMovement && !EnemyData.HasAction)
                {
                    EndTurn();
                }
            }
        }

        public void SetDifficulty(float difficultyMultiplier)
        {
            _enemyData.Health = Mathf.RoundToInt(_enemyData.Health * difficultyMultiplier);
            _enemyData.MaxHealth = Mathf.RoundToInt(_enemyData.MaxHealth * difficultyMultiplier);
            _enemyData.BonusToHit = Mathf.RoundToInt(_enemyData.BonusToHit * difficultyMultiplier);
            CalculateStatBonuses();
        }

        private bool IsAttackBlocked(OverlayTile start, OverlayTile end)
        {
            var attackLine = new Ray(start.gridLocation, (end.gridLocation - start.gridLocation));
            var distanceToPlayer = GetDistance(start, end);
            var attackLineColliders = new List<Collider2D>();
            var attackLineHits = Physics2D.GetRayIntersectionAll(attackLine, distanceToPlayer).ToList();
            foreach (var hit in attackLineHits)
            {
                attackLineColliders.Add(hit.collider);
            }
            var attackLineTiles = (from overlayTile in _gameManager.MapMan.Map.Values
                                   from col
                in attackLineColliders
                                   where overlayTile.GetComponent<Collider2D>() == col
                                   select overlayTile).ToList();
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
            return _gameManager.Party.PartyObjects.Select(player =>
                  _gameManager.PathFinder.FindPath(_standingOnTile, player.GetComponent<PlayerController>().StandingOnTile,
                      new List<OverlayTile>(), false)).ToList();
        }
        private List<List<OverlayTile>> GetPlayerPaths(List<GameObject> playersInRange)
        {
            return playersInRange.Select(player =>
                    _gameManager.PathFinder.FindPath(_standingOnTile, player.GetComponent<PlayerController>().StandingOnTile,
                        new List<OverlayTile>(), false)).ToList();
        }
        private List<GameObject> GetPlayersInRangedRange(OverlayTile tile)
        {
            var playersInRange = new List<GameObject>();
            var inRangeTiles = _gameManager.RangeFinder.GetRangeTiles(tile.Grid2DLocation, _enemyData.EquippedWeapon.weaponRange);
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
                var info = p.GetComponent<PlayerController>();
                if (info.CurrentHealth >= lowest) continue;
                lowest = info.CurrentHealth;
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
            playerPaths.Sort((a, b) => a.Count - b.Count);
            var previousPath = new List<OverlayTile>(200);
            var closestPlayers = new List<GameObject>();
            foreach (var path in playerPaths) // find the shortest paths
            {
                if (path.Count! < previousPath.Count)
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
            playerPaths.Sort((a, b) => a.Count - b.Count);
            var bestPath = playerPaths.First();

            var bestPlayer = bestPath.Last().GetPlayerOnTile();
            return bestPlayer;
        }
        private GameObject GetBestPlayer(List<GameObject> playersInRange)
        {
            var playerPaths = GetPlayerPaths(playersInRange);
            playerPaths.Sort((a, b) => a.Count - b.Count);
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
            return (from ally in _allies
                    let activeTile = ally.GetComponent<EnemyController>().StandingOnTile
                    where areaOfEffectTiles.Contains(activeTile)
                    select ally).ToList();
        }
        private float CalculateMovementValue(List<OverlayTile> tempMovePath)
        {
            var playersAlongPath = new List<GameObject>();
            foreach (var p in from tile in tempMovePath
                              select tile.GetNeighbors()
                     into inMeleeRange
                              from t in inMeleeRange
                              from p in _gameManager.Party.PartyObjects
                              where p.GetComponent<PlayerController>().StandingOnTile == t &&
                                    !playersAlongPath.Contains(p)
                              let info = p.GetComponent<PlayerController>().characterData
                              where p.GetComponent<PlayerController>().StandingOnTile == t &&
                                                                               playersAlongPath.Contains(p)
                              select p)
            {
                playersAlongPath.Add(p);
            }
            return _enemyData.Speed - playersAlongPath.Count;
        }
        private float CalculateRangedMovementValue(List<OverlayTile> tempMovePath)
        {
            var newTile = _standingOnTile;
            if (tempMovePath.Count > 0)
            {
                newTile = tempMovePath.Last();
            }
            var tilesInRange = _gameManager.RangeFinder.GetRangeTiles(newTile.Grid2DLocation, _enemyData.Movement);
            var playersInRange = GetPlayersInRange(_standingOnTile, _enemyData.Movement);
            var value = 0f;
            switch (playersInRange.Count)
            {
                case 0:
                    {
                        value = _enemyData.Speed;
                        break;
                    }
                case 1:
                    {
                        var playerPaths = GetPlayerPaths(playersInRange);
                        value = _enemyData.Speed + playerPaths[0].Count;
                        break;
                    }
                case > 1:
                    {
                        var playerPaths = GetPlayerPaths(playersInRange);
                        var combinedPathValue = playerPaths.Sum(path => path.Count);
                        value = _enemyData.Speed + combinedPathValue;
                        break;
                    }
            }
            return value;
        }

        protected List<GameObject> GetPlayersInRange(OverlayTile centerTile, int range)
        {
            var tilesInRange = _gameManager.RangeFinder.GetRangeTiles(centerTile.Grid2DLocation, range);
            return _gameManager.Party.PartyObjects.Where(player =>
                tilesInRange.Contains(player.GetComponent<PlayerController>().StandingOnTile)).ToList();
        }

        private float CalculateMeleeAttackValue(PlayerController playerInfo)
        {
            float hitValue;
            float dmgDifference;
            var playerHealth = playerInfo.CurrentHealth;
            if ((_enemyData.EquippedWeapon.averageDmg + _enemyData.Modifiers[_enemyData.EquippedWeapon.weaponStat]) >
                playerHealth)
            {
                hitValue = _enemyData.EquippedWeapon.averageDmg +
                            _enemyData.Modifiers[_enemyData.EquippedWeapon.weaponStat];
                dmgDifference = playerHealth - hitValue;
                return hitValue + dmgDifference;
            }
            hitValue = _enemyData.EquippedWeapon.averageDmg +
                       _enemyData.Modifiers[_enemyData.EquippedWeapon.weaponStat];
            dmgDifference = hitValue - playerHealth;
            return hitValue + dmgDifference + 100f;
        }

        private float CalculateRangedAttackValue(PlayerController playerInfo, int distanceToPlayer)
        {
            if (distanceToPlayer < 2 && _enemyData.EquippedWeapon.averageDmg - playerInfo.CurrentHealth > 0)
            {
                return -10000;
            }
            var attackValue = distanceToPlayer;
            return attackValue;
        }

        private List<GameObject> GetPlayersInMeleeRange(OverlayTile tile)
        {
            return GetPlayersInRange(tile, _enemyData.EquippedWeapon.weaponRange);
        }

        private float CalculateAbilityAttackValue(PlayerController playerInfo, Ability ability,
            List<GameObject> playersInAbilityRange, List<GameObject> alliesAffected)
        {
            float hitValue;
            var playersAffected = playersInAbilityRange.Count;
            if ((ability.averageDmg + _enemyData.Modifiers[ability.stat])! > playerInfo.CurrentHealth && alliesAffected.Count == 0)
            {
                hitValue = (ability.averageDmg + _enemyData.Modifiers[ability.stat]) - playerInfo.CurrentHealth;
                var dmgDifference = Math.Abs(hitValue - playerInfo.CurrentHealth);
                return hitValue + playersAffected + dmgDifference;
            }
            if ((ability.averageDmg + _enemyData.Modifiers[ability.stat])! > playerInfo.CurrentHealth && alliesAffected.Count > 0)
            {
                hitValue = (ability.averageDmg + _enemyData.Modifiers[ability.stat]) - playerInfo.CurrentHealth;
                var dmgDifference = Math.Abs(hitValue - playerInfo.CurrentHealth);
                return (hitValue + playersAffected + dmgDifference) * .25f;
            }
            if ((ability.averageDmg + _enemyData.Modifiers[ability.stat]) > playerInfo.CurrentHealth && alliesAffected.Count == 0)
            {
                hitValue = (ability.averageDmg + _enemyData.Modifiers[ability.stat]) - playerInfo.CurrentHealth;
                var dmgDifference = Math.Abs(hitValue - playerInfo.CurrentHealth);
                return hitValue + playersAffected + dmgDifference + 100f;
            }
            if ((ability.averageDmg + _enemyData.Modifiers[ability.stat]) > playerInfo.CurrentHealth && alliesAffected.Count > 0)
            {
                hitValue = (ability.averageDmg + _enemyData.Modifiers[ability.stat]) - playerInfo.CurrentHealth;
                var dmgDifference = Math.Abs(hitValue - playerInfo.CurrentHealth);
                return (hitValue + playersAffected + dmgDifference + 100f) * .25f;
            }
            return 0f;
        }

        private void AttackPlayer(GameObject playerToAttack)
        {
            if (playerToAttack)
            {
                switch (_enemyData.AttackType)
                {
                    case "Melee":
                        {
                            var playersInMeleeRange = GetPlayersInMeleeRange(_standingOnTile);
                            if (playersInMeleeRange.Contains(playerToAttack))
                            {
                                _combatController.AttackOtherCharacter(this, playerToAttack.GetComponent<PlayerController>());//TODO play animation
                                EnemyData.HasAction = false;
                                break;
                            }

                            EnemyData.HasAction = false;
                            break;
                        }
                    case "Ranged":
                        {
                            if (playerToAttack)
                            {
                                _combatController.AttackOtherCharacter(this, playerToAttack.GetComponent<PlayerController>());//TODO play animation

                                EnemyData.HasAction = false;
                                break;
                            }

                            EnemyData.HasAction = false;
                            break;
                        }
                }
            }
            EnemyData.HasAction = false;
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
            if (gameObject && tile != _standingOnTile)
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
                _isMoving = false;
                EnemyData.HasMovement = false;
            }
        }

        /** MoveAlongPath()
         * description: gets the enemies speed of movement and then moves the player
         * @return void
         */
        private void MoveAlongPath()
        {
            // slows movement for the visual
            var step = _enemyData.Speed * Time.deltaTime;

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
            EnemyData.HasMovement = false;
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
                                                             && overlayTile != _standingOnTile)
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
            _standingOnTile = tile;
        }

        private void CalculateStatBonuses()
        {
            for (var i = 0; i < _enemyData.Stats.Count; i++)
            {
                var stat = _enemyData.Stats[i];
                if (stat == 1)
                {
                    _enemyData.Modifiers.Add(_enemyData.StatType[i], -5);
                }
                if (stat is >= 2 and <= 3)
                {
                    _enemyData.Modifiers.Add(_enemyData.StatType[i], -4);
                }
                if (stat is >= 4 and <= 5)
                {
                    _enemyData.Modifiers.Add(_enemyData.StatType[i], -3);
                }
                if (stat is >= 6 and <= 7)
                {
                    _enemyData.Modifiers.Add(_enemyData.StatType[i], -2);
                }
                if (stat is >= 8 and <= 9)
                {
                    _enemyData.Modifiers.Add(_enemyData.StatType[i], -1);
                }
                if (stat is >= 10 and <= 11)
                {
                    _enemyData.Modifiers.Add(_enemyData.StatType[i], 0);
                }
                if (stat is >= 12 and <= 13)
                {
                    _enemyData.Modifiers.Add(_enemyData.StatType[i], 1);
                }
                if (stat is >= 14 and <= 15)
                {
                    _enemyData.Modifiers.Add(_enemyData.StatType[i], 2);
                }
                if (stat is >= 16 and <= 17)
                {
                    _enemyData.Modifiers.Add(_enemyData.StatType[i], 3);
                }
                if (stat is >= 18 and <= 19)
                {
                    _enemyData.Modifiers.Add(_enemyData.StatType[i], 4);
                }
                if (stat is >= 20 and <= 21)
                {
                    _enemyData.Modifiers.Add(_enemyData.StatType[i], 5);
                }
                if (stat is >= 22 and <= 23)
                {
                    _enemyData.Modifiers.Add(_enemyData.StatType[i], 6);
                }
                if (stat is >= 24 and <= 25)
                {
                    _enemyData.Modifiers.Add(_enemyData.StatType[i], 7);
                }
                if (stat is >= 26 and <= 27)
                {
                    _enemyData.Modifiers.Add(_enemyData.StatType[i], 8);
                }
                if (stat is >= 28 and <= 29)
                {
                    _enemyData.Modifiers.Add(_enemyData.StatType[i], 9);
                }
                if (stat == 30)
                {
                    _enemyData.Modifiers.Add(_enemyData.StatType[i], 10);
                }

            }
        }

        public void Initialize(EnemyData enemyData)
        {
            _enemyData = enemyData;
            // Set up AI strategy based on enemy type
            _aiStrategy = AIStrategyFactory.GetStrategy(_enemyData.AttackType);

            // Initialize from data if available
            if (_enemyData != null)
            {
                CalculateStatBonuses();
            }
            OnEnemyLoaded?.Invoke();
        }

        protected override void OnCombatStarted()
        {
            // Reset for combat
            EnemyData.HasMovement = true;
            EnemyData.HasAction = true;
            EnemyData.HasBonusAction = true;
            EnemyData.HasReaction = true;
            _path.Clear();
            _isMoving = false;
            _strategyFound = false;
        }
        protected override void OnCombatEnded()
        {
            // Reset after combat
            _path.Clear();
            _isMoving = false;
            _strategyFound = false;
        }

        public bool CheckIfTileIsPlayerTile(OverlayTile tile)
        {
            foreach (var player in _gameManager.Party.Party)
            {
                // Check if the tile matches the player's standing tile
                if (tile == player.StandingOnTile)
                {
                    return true;
                }
            }
            return false;
        }

        // Utility method to get players in range for strategies
        public List<GameObject> GetPartyMembersInRange(OverlayTile tile, int range)
        {
            var tilesInRange = _gameManager.RangeFinder.GetRangeTiles(tile.Grid2DLocation, range);
            var players = new List<GameObject>();
            foreach (var t in tilesInRange)
            {
                if (CheckIfTileIsPlayerTile(t))
                {
                    var player = t.GetPlayerOnTile();
                    if (player != null)
                    {
                        players.Add(player);
                    }
                }
            }
            return players;
        }
    }
}
