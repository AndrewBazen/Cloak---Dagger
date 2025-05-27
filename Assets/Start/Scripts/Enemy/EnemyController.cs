using System;
using System.Collections.Generic;
using System.Linq;
using Start.Scripts.Game;
using Start.Scripts.Inventory;
using Start.Scripts.Map;
using Start.Scripts.Enemy.Strategies;
using UnityEngine;
using Start.Scripts.Combat;
using Start.Scripts.Character;
using Start.Scripts.BaseClasses;
using Random = System.Random;
using System.ComponentModel;

namespace Start.Scripts.Enemy
{
    public class EnemyController : Controller, INotifyPropertyChanged
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

        public bool HasMovement
        {
            get => hasMovement;
            set
            {
                hasMovement = value;
                OnPropertyChanged(nameof(HasMovement));
            }
        }

        public bool HasAttack
        {
            get => hasAttack;
            set
            {
                hasAttack = value;
                OnPropertyChanged(nameof(HasAttack));
            }
        }

        private const float InitialMovementValue = 20f;
        private IReadOnlyList<GameObject> _allies;
        private CombatController _combatController;
        private List<OverlayTile> _path;

        private List<OverlayTile> _rangeFinderTiles;
        private List<OverlayTile> _rangeTileDistances;
        private Dictionary<string, int> _statBonuses;
        private int _spellSlotsAvailable;
        private Strategy _strategy;
        private bool _isMoving = false;
        private bool _strategyFound = false;

        // GameManager integration
        private IAIStrategy _aiStrategy;

        // runs when the object becomes awake
        private void Awake()
        {
            _combatController = GetComponent<CombatController>();
            _statBonuses = new Dictionary<string, int>();
        }

        // Start is called before the first frame update
        protected override void Start()
        {
            InitializeController();
            SetEnemyValues();
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
            if (_combatController.isTurn)
            {
                // gets player object, range, and tile.
                if (!_strategyFound)
                {
                    _strategy = _aiStrategy.EvaluateStrategy(this);
                    _strategyFound = true;
                }
                if (hasMovement && _strategyFound)
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
                  _gameManager.PathFinder.FindPath(standingOnTile, player.GetComponent<PlayerController>().StandingOnTile,
                      new List<OverlayTile>(), false)).ToList();
        }
        private List<List<OverlayTile>> GetPlayerPaths(List<GameObject> playersInRange)
        {
            return playersInRange.Select(player =>
                    _gameManager.PathFinder.FindPath(standingOnTile, player.GetComponent<PlayerController>().StandingOnTile,
                        new List<OverlayTile>(), false)).ToList();
        }
        private List<GameObject> GetPlayersInRangedRange(OverlayTile tile)
        {
            var playersInRange = new List<GameObject>();
            var inRangeTiles = _gameManager.RangeFinder.GetRangeTiles(tile.Grid2DLocation, weapon.weaponRange);
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
                    let activeTile = ally.GetComponent<EnemyController>().standingOnTile
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
            return InitialMovementValue - playersAlongPath.Count;
        }
        private float CalculateRangedMovementValue(List<OverlayTile> tempMovePath)
        {
            var newTile = standingOnTile;
            if (tempMovePath.Count > 0)
            {
                newTile = tempMovePath.Last();
            }
            var tilesInRange = _gameManager.RangeFinder.GetRangeTiles(newTile.Grid2DLocation, movement);
            var playersInRange = GetPlayersInRange(standingOnTile, movement);
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
        protected List<GameObject> GetPlayersInRange(OverlayTile centerTile, int range)
        {
            var tilesInRange = _gameManager.RangeFinder.GetRangeTiles(centerTile.Grid2DLocation, range);
            return _gameManager.Party.PartyObjects.Where(player =>
                tilesInRange.Contains(player.GetComponent<PlayerController>().StandingOnTile)).ToList();
        }
        private float CalculateMeleeAttackValue(PlayerController playerInfo)
        {
            var hitValue = 0f;
            var dmgDifference = 0f;
            var playerHealth = playerInfo.CurrentHealth;
            if ((weapon.averageDmg + _statBonuses[weapon.weaponStat])! >
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
            return hitValue + dmgDifference + 100f;
        }
        private float CalculateRangedAttackValue(PlayerController playerInfo, int distanceToPlayer)
        {
            if (distanceToPlayer < 2 && weapon.averageDmg - playerInfo.CurrentHealth > 0)
            {
                return -10000;
            }
            var attackValue = distanceToPlayer;
            return attackValue;
        }
        private List<GameObject> GetPlayersInMeleeRange(OverlayTile tile)
        {
            return GetPlayersInRange(tile, weapon.weaponRange);
        }
        private float CalculateAbilityAttackValue(PlayerController playerInfo, Ability ability,
            List<GameObject> playersInAbilityRange, List<GameObject> alliesAffected)
        {
            float hitValue;
            var playersAffected = playersInAbilityRange.Count;
            if ((ability.averageDmg + _statBonuses[ability.stat])! > playerInfo.CurrentHealth && alliesAffected.Count == 0)
            {
                hitValue = (ability.averageDmg + _statBonuses[ability.stat]) - playerInfo.CurrentHealth;
                var dmgDifference = Math.Abs(hitValue - playerInfo.CurrentHealth);
                return hitValue + playersAffected + dmgDifference;
            }
            if ((ability.averageDmg + _statBonuses[ability.stat])! > playerInfo.CurrentHealth && alliesAffected.Count > 0)
            {
                hitValue = (ability.averageDmg + _statBonuses[ability.stat]) - playerInfo.CurrentHealth;
                var dmgDifference = Math.Abs(hitValue - playerInfo.CurrentHealth);
                return (hitValue + playersAffected + dmgDifference) * .25f;
            }
            if ((ability.averageDmg + _statBonuses[ability.stat]) > playerInfo.CurrentHealth && alliesAffected.Count == 0)
            {
                hitValue = (ability.averageDmg + _statBonuses[ability.stat]) - playerInfo.CurrentHealth;
                var dmgDifference = Math.Abs(hitValue - playerInfo.CurrentHealth);
                return hitValue + playersAffected + dmgDifference + 100f;
            }
            if ((ability.averageDmg + _statBonuses[ability.stat]) > playerInfo.CurrentHealth && alliesAffected.Count > 0)
            {
                hitValue = (ability.averageDmg + _statBonuses[ability.stat]) - playerInfo.CurrentHealth;
                var dmgDifference = Math.Abs(hitValue - playerInfo.CurrentHealth);
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
                                _combatController.AttackOtherCharacter(this, playerToAttack.GetComponent<PlayerController>());//TODO play animation
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
                                _combatController.AttackOtherCharacter(this, playerToAttack.GetComponent<PlayerController>());//TODO play animation

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
                }
                if (stat is >= 2 and <= 3)
                {
                    _statBonuses.Add(statType[i], -4);
                }
                if (stat is >= 4 and <= 5)
                {
                    _statBonuses.Add(statType[i], -3);
                }
                if (stat is >= 6 and <= 7)
                {
                    _statBonuses.Add(statType[i], -2);
                }
                if (stat is >= 8 and <= 9)
                {
                    _statBonuses.Add(statType[i], -1);
                }
                if (stat is >= 10 and <= 11)
                {
                    _statBonuses.Add(statType[i], 0);
                }
                if (stat is >= 12 and <= 13)
                {
                    _statBonuses.Add(statType[i], 1);
                }
                if (stat is >= 14 and <= 15)
                {
                    _statBonuses.Add(statType[i], 2);
                }
                if (stat is >= 16 and <= 17)
                {
                    _statBonuses.Add(statType[i], 3);
                }
                if (stat is >= 18 and <= 19)
                {
                    _statBonuses.Add(statType[i], 4);
                }
                if (stat is >= 20 and <= 21)
                {
                    _statBonuses.Add(statType[i], 5);
                }
                if (stat is >= 22 and <= 23)
                {
                    _statBonuses.Add(statType[i], 6);
                }
                if (stat is >= 24 and <= 25)
                {
                    _statBonuses.Add(statType[i], 7);
                }
                if (stat is >= 26 and <= 27)
                {
                    _statBonuses.Add(statType[i], 8);
                }
                if (stat is >= 28 and <= 29)
                {
                    _statBonuses.Add(statType[i], 9);
                }
                if (stat == 30)
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

        public bool CheckIfTileIsPlayerTile(OverlayTile tile)
        {
            foreach (var player in _gameManager.Party.PartyControllers)
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
