using System;
using System.Collections.Generic;
using System.Linq;
using Start.Scripts.Character;
using Start.Scripts.Combat;
using Start.Scripts.Dice;
using Start.Scripts.Enemy;
using Start.Scripts.Inventory;
using Start.Scripts.Map;
using Start.Scripts.Serialization;
using Start.Scripts.Services;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Start.Scripts.Game
{
    /// <summary>
    /// Central manager for the game. Handles game state, initialization, and serves as the focal point for
    /// communication between different systems.
    /// </summary>
    public class GameManager : MonoBehaviour 
    {
        #region Singleton
        private static GameManager _instance;
        public static GameManager Instance => _instance;
        #endregion

        #region Events
        // Game state events
        public event Action OnGameInitialized;
        public event Action OnGameStarted;
        public event Action OnGamePaused;
        public event Action OnGameResumed;
        public event Action OnGameOver;
        public event Action OnLevelComplete;

        // Combat events
        public event Action OnCombatStarted;
        public event Action OnCombatEnded;
        public event Action<int> OnTurnChanged;
        public event Action<CharacterInfo, int> OnCharacterDamaged;
        public event Action<EnemyController, int> OnEnemyDamaged;
        public event Action<CharacterInfo> OnCharacterDied;
        public event Action<EnemyController> OnEnemyDied;

        // Player events
        public event Action<CharacterInfo> OnPlayerSpawned;
        public event Action<List<CharacterInfo>> OnPartyUpdated;
        public event Action<CharacterInfo, int> OnPlayerLeveledUp;

        // Map events
        public event Action OnMapGenerated;
        public event Action<Vector2Int> OnPlayerMoved;
        #endregion

        #region References
        [Header("Prefabs")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject damageTextPrefab;

        [Header("Containers")]
        [SerializeField] private GameObject playerContainer;
        [SerializeField] private GameObject enemyContainer;
        [SerializeField] private GameObject mapContainer;

        [Header("Systems")]
        [SerializeField] private GameObject cameraController;
        [SerializeField] private GameObject mapManager;
        [SerializeField] private GameObject uiManager;
        #endregion

        #region State
        // Game state
        public enum GameState { MainMenu, Initialization, Exploration, Combat, Paused, GameOver }
        private GameState _currentGameState;
        public GameState CurrentGameState => _currentGameState;

        // Combat state
        private TurnOrder _turnOrder;
        private CombatController _combatController;
        private bool _isCombatActive;
        public bool IsCombatActive => _isCombatActive;

        // Player/party state
        private List<GameObject> _partyObjects = new List<GameObject>();
        private List<CharacterInfo> _partyMembers = new List<CharacterInfo>();
        public List<CharacterInfo> PartyMembers => _partyMembers;
        private CharacterInfo _activePlayer;
        public CharacterInfo ActivePlayer => _activePlayer;

        // Enemy state
        private List<GameObject> _enemyObjects = new List<GameObject>();
        private List<EnemyController> _enemies = new List<EnemyController>();
        public List<EnemyController> Enemies => _enemies;

        // Level state
        private int _currentLevel = 1;
        public int CurrentLevel => _currentLevel;
        private float _difficultyMultiplier = 1.0f;
        #endregion

        #region Services
        private SaveSystem _saveSystem;
        private SpawnService _spawnService;
        private DiceRoll _diceRoll;
        #endregion

        #region Initialization
        private void Awake()
        {
            // Singleton pattern
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize services
            _saveSystem = new SaveSystem();
            _spawnService = new SpawnService();
            _diceRoll = new DiceRoll();

            // Initialize state
            _currentGameState = GameState.Initialization;
            _partyMembers = new List<CharacterInfo>();
            _enemies = new List<EnemyController>();
            _isCombatActive = false;

            // Get references if not set in inspector
            if (playerContainer == null)
                playerContainer = GameObject.FindGameObjectWithTag("Players");
            if (enemyContainer == null)
                enemyContainer = GameObject.FindGameObjectWithTag("Enemies");
        }

        private void Start()
        {
            // Subscribe to GameEvents
            GameEvents.current.OnLoadEvent += OnLevelLoaded;
            GameEvents.current.OnEscPressed += TogglePause;
            
            // Set initial game state
            _currentGameState = GameState.MainMenu;
            
            // Notify that game is initialized
            OnGameInitialized?.Invoke();
        }
        #endregion

        #region Game Flow
        public void StartNewGame()
        {
            // Reset game state
            _currentLevel = 1;
            _difficultyMultiplier = 1.0f;
            
            // Clear any existing party/enemies
            ClearParty();
            ClearEnemies();
            
            // Load the first level
            LoadLevel(_currentLevel);
            
            // Set game state
            _currentGameState = GameState.Exploration;
            OnGameStarted?.Invoke();
        }

        public void LoadGame(string saveName)
        {
            // Load saved game data
            var saveData = _saveSystem.LoadGameData(saveName);
            if (saveData == null)
            {
                Debug.LogError("Failed to load saved game data");
                return;
            }
            
            // Set level and difficulty
            _currentLevel = saveData.level;
            _difficultyMultiplier = saveData.difficultyMultiplier;
            
            // Load the level
            LoadLevel(_currentLevel);
            
            // Load party members
            LoadPartyFromSave(saveData.party);
            
            // Set game state
            _currentGameState = GameState.Exploration;
            OnGameStarted?.Invoke();
        }

        public void SaveGame()
        {
            var saveName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            SaveGameAs(saveName);
        }

        public void SaveGameAs(string saveName)
        {
            _saveSystem.SaveGameData(saveName, _partyMembers, _currentLevel, _difficultyMultiplier);
        }

        private void LoadLevel(int level)
        {
            // Load the appropriate scene for the level
            string sceneName = $"Level_{level}";
            SceneManager.LoadScene(sceneName);
        }

        private void OnLevelLoaded()
        {
            // Find required objects in the scene
            if (mapManager == null)
                mapManager = GameObject.FindGameObjectWithTag("MapManager");
            
            // Initialize turn order system if it doesn't exist
            if (_turnOrder == null)
            {
                var turnOrderObject = GameObject.FindGameObjectWithTag("TurnController");
                if (turnOrderObject != null)
                    _turnOrder = turnOrderObject.GetComponent<TurnOrder>();
            }
            
            // Initialize combat controller if it doesn't exist
            if (_combatController == null)
            {
                _combatController = FindObjectOfType<CombatController>();
            }
            
            // Spawn initial enemies for the level
            SpawnEnemiesForLevel(_currentLevel);
            
            // Update camera target if active player exists
            if (_activePlayer != null && cameraController != null)
            {
                // Set camera to follow active player
                // Implementation depends on your camera controller
            }
            
            // Notify that map is generated
            OnMapGenerated?.Invoke();
        }

        public void CompleteLevel()
        {
            _currentLevel++;
            _difficultyMultiplier += 0.2f;
            
            // Save party state between levels
            SaveGame();
            
            // Notify level complete
            OnLevelComplete?.Invoke();
            
            // Load next level
            LoadLevel(_currentLevel);
        }

        public void TogglePause()
        {
            if (_currentGameState == GameState.Paused)
            {
                _currentGameState = GameState.Exploration;
                Time.timeScale = 1.0f;
                OnGameResumed?.Invoke();
            }
            else
            {
                _currentGameState = GameState.Paused;
                Time.timeScale = 0.0f;
                OnGamePaused?.Invoke();
            }
        }

        public void EndGame()
        {
            _currentGameState = GameState.GameOver;
            OnGameOver?.Invoke();
            
            // Load game over scene
            SceneManager.LoadScene("GameOver");
        }
        #endregion

        #region Party Management
        public CharacterInfo SpawnPlayer(Vector3 position, PlayerClass playerClass = null)
        {
            GameObject playerObject = Instantiate(playerPrefab, position, Quaternion.identity, playerContainer.transform);
            CharacterInfo playerInfo = playerObject.GetComponent<CharacterInfo>();
            
            if (playerInfo != null)
            {
                // Initialize player with class if provided
                if (playerClass != null)
                    playerInfo.playerClass = playerClass;
                
                // Add to party tracking
                _partyObjects.Add(playerObject);
                _partyMembers.Add(playerInfo);
                
                // If no active player is set, make this one active
                if (_activePlayer == null)
                    _activePlayer = playerInfo;
                
                // Notify of player spawn
                OnPlayerSpawned?.Invoke(playerInfo);
                OnPartyUpdated?.Invoke(_partyMembers);
            }
            
            return playerInfo;
        }

        public void AddToParty(CharacterInfo character)
        {
            if (!_partyMembers.Contains(character))
            {
                _partyMembers.Add(character);
                _partyObjects.Add(character.gameObject);
                OnPartyUpdated?.Invoke(_partyMembers);
            }
        }

        public void RemoveFromParty(CharacterInfo character)
        {
            if (_partyMembers.Contains(character))
            {
                _partyMembers.Remove(character);
                _partyObjects.Remove(character.gameObject);
                
                // If removing active player, select a new one
                if (_activePlayer == character && _partyMembers.Count > 0)
                    _activePlayer = _partyMembers[0];
                
                OnPartyUpdated?.Invoke(_partyMembers);
            }
        }

        public void SetActivePlayer(CharacterInfo character)
        {
            if (_partyMembers.Contains(character))
            {
                _activePlayer = character;
                
                // Update camera to follow new active player
                if (cameraController != null)
                {
                    // Implementation depends on your camera controller
                }
            }
        }

        private void ClearParty()
        {
            foreach (var playerObject in _partyObjects)
            {
                if (playerObject != null)
                    Destroy(playerObject);
            }
            
            _partyObjects.Clear();
            _partyMembers.Clear();
            _activePlayer = null;
            
            OnPartyUpdated?.Invoke(_partyMembers);
        }

        private void LoadPartyFromSave(List<CharacterInfo> savedParty)
        {
            ClearParty();
            
            foreach (var savedCharacter in savedParty)
            {
                // Create a new character with saved data
                Vector3 position = Vector3.zero; // Default position, adjust as needed
                CharacterInfo newCharacter = SpawnPlayer(position);
                
                // Copy saved data to new character
                // Implementation depends on your save system
            }
        }
        #endregion

        #region Enemy Management
        public EnemyController SpawnEnemy(Vector3 position, EnemyData enemyData = null)
        {
            GameObject enemyObject = Instantiate(enemyPrefab, position, Quaternion.identity, enemyContainer.transform);
            EnemyController enemyController = enemyObject.GetComponent<EnemyController>();
            
            if (enemyController != null)
            {
                // Initialize enemy with data if provided
                if (enemyData != null)
                    enemyController.data = enemyData;
                
                // Apply difficulty scaling
                ApplyDifficultyToEnemy(enemyController);
                
                // Add to enemy tracking
                _enemyObjects.Add(enemyObject);
                _enemies.Add(enemyController);
                
                // Notify of enemy spawn
                GameEvents.current.OnEnemySpawn?.Invoke();
                
                // Update enemies reference in other components
                UpdateEnemiesReferences();
            }
            
            return enemyController;
        }

        private void ApplyDifficultyToEnemy(EnemyController enemy)
        {
            // Scale enemy stats based on difficulty
            enemy.maxHealth = Mathf.RoundToInt(enemy.maxHealth * _difficultyMultiplier);
            enemy.health = enemy.maxHealth;
            enemy.bonusToHit = Mathf.RoundToInt(enemy.bonusToHit * _difficultyMultiplier);
            // Apply other stat scaling as needed
        }

        private void UpdateEnemiesReferences()
        {
            // Notify any components that need the updated enemy list
            GameEvents.current.OnEnemiesChanged?.Invoke(_enemyObjects);
        }

        public void RemoveEnemy(EnemyController enemy)
        {
            if (_enemies.Contains(enemy))
            {
                _enemies.Remove(enemy);
                _enemyObjects.Remove(enemy.gameObject);
                
                // Check if all enemies are defeated
                if (_enemies.Count == 0 && _isCombatActive)
                {
                    EndCombat(true);
                }
                
                UpdateEnemiesReferences();
                OnEnemyDied?.Invoke(enemy);
            }
        }

        private void ClearEnemies()
        {
            foreach (var enemyObject in _enemyObjects)
            {
                if (enemyObject != null)
                    Destroy(enemyObject);
            }
            
            _enemyObjects.Clear();
            _enemies.Clear();
            
            UpdateEnemiesReferences();
        }

        private void SpawnEnemiesForLevel(int level)
        {
            // Clear any existing enemies
            ClearEnemies();
            
            // Spawn enemies based on level
            int enemyCount = 2 + level;
            
            for (int i = 0; i < enemyCount; i++)
            {
                // Find a valid spawn position
                Vector3 spawnPosition = FindEnemySpawnPosition();
                
                // Create enemy type based on level
                EnemyData enemyData = CreateEnemyDataForLevel(level);
                
                // Spawn the enemy
                SpawnEnemy(spawnPosition, enemyData);
            }
        }

        private Vector3 FindEnemySpawnPosition()
        {
            // Implementation depends on your map system
            // Find a valid tile position for spawning enemies
            return new Vector3(UnityEngine.Random.Range(-5, 5), UnityEngine.Random.Range(-5, 5), 0);
        }

        private EnemyData CreateEnemyDataForLevel(int level)
        {
            // Create enemy data appropriate for the current level
            // Implementation depends on your enemy data system
            EnemyData data = new EnemyData();
            // Configure enemy based on level
            return data;
        }
        #endregion

        #region Combat Management
        public void StartCombat()
        {
            if (_isCombatActive)
                return;
            
            _isCombatActive = true;
            _currentGameState = GameState.Combat;
            
            // Initialize turn order
            if (_turnOrder != null)
            {
                _turnOrder.InitializeTurnOrder(_partyMembers, _enemies);
            }
            
            OnCombatStarted?.Invoke();
        }

        public void EndCombat(bool victory)
        {
            if (!_isCombatActive)
                return;
            
            _isCombatActive = false;
            _currentGameState = GameState.Exploration;
            
            if (victory)
            {
                // Reward players
                RewardPartyForCombat();
            }
            
            OnCombatEnded?.Invoke();
        }

        private void RewardPartyForCombat()
        {
            // Calculate XP and other rewards
            int baseXP = 50;
            int xpPerEnemy = 25;
            int totalXP = baseXP + xpPerEnemy * _enemies.Count;
            
            // Apply level multiplier
            totalXP = Mathf.RoundToInt(totalXP * (1 + (_currentLevel * 0.1f)));
            
            // Distribute XP to party members
            int xpPerMember = totalXP / _partyMembers.Count;
            foreach (var member in _partyMembers)
            {
                GiveExperienceToCharacter(member, xpPerMember);
            }
            
            // Other rewards like gold, items, etc.
        }

        private void GiveExperienceToCharacter(CharacterInfo character, int experience)
        {
            character.experience += experience;
            
            // Check for level up
            if (character.experience >= character.maxExperience)
            {
                LevelUpCharacter(character);
            }
        }

        private void LevelUpCharacter(CharacterInfo character)
        {
            // Level up logic
            int oldLevel = character.level;
            int newLevel = oldLevel + 1;
            
            // Update stats
            character.level = newLevel;
            character.maxHealth += 5 + (character.stats[2] / 2); // Con bonus
            character.health = character.maxHealth;
            character.maxMana += 3 + (character.stats[3] / 2); // Int bonus
            character.mana = character.maxMana;
            
            // Reset experience for next level
            character.experience = 0;
            character.maxExperience = CalculateExperienceForLevel(newLevel);
            
            // Notify of level up
            OnPlayerLeveledUp?.Invoke(character, newLevel);
        }

        private int CalculateExperienceForLevel(int level)
        {
            // Experience curve calculation
            return 100 * level * level;
        }

        public void DamageCharacter(CharacterInfo character, int damage)
        {
            if (character == null)
                return;
            
            character.health -= damage;
            OnCharacterDamaged?.Invoke(character, damage);
            
            if (character.health <= 0)
            {
                KillCharacter(character);
            }
        }

        public void DamageEnemy(EnemyController enemy, int damage)
        {
            if (enemy == null)
                return;
            
            enemy.health -= damage;
            OnEnemyDamaged?.Invoke(enemy, damage);
            
            if (enemy.health <= 0)
            {
                KillEnemy(enemy);
            }
        }

        private void KillCharacter(CharacterInfo character)
        {
            OnCharacterDied?.Invoke(character);
            RemoveFromParty(character);
            
            // Check if all party members are dead
            if (_partyMembers.Count == 0)
            {
                EndGame();
            }
        }

        private void KillEnemy(EnemyController enemy)
        {
            OnEnemyDied?.Invoke(enemy);
            RemoveEnemy(enemy);
        }
        #endregion

        #region Utility Methods
        public void RollInitiative(CharacterInfo character)
        {
            if (_diceRoll == null)
                _diceRoll = new DiceRoll();
            
            int initiativeRoll = _diceRoll.RollDice("D20", 1).Keys.First();
            character.initiative = initiativeRoll;
        }

        public void RollInitiative(EnemyController enemy)
        {
            if (_diceRoll == null)
                _diceRoll = new DiceRoll();
            
            int initiativeRoll = _diceRoll.RollDice("D20", 1).Keys.First();
            enemy.initiative = initiativeRoll;
        }
        
        // Add more utility methods as needed
        #endregion
    }
}