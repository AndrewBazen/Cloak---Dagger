using System.Collections.Generic;
using Start.Scripts.Character;
using Start.Scripts.Dice;
using Start.Scripts.Enemy;
using Start.Scripts.Game;
using Start.Scripts.Map;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vector2 = System.Numerics.Vector2;

namespace Start.Scripts.Combat
{
    public class CombatController : MonoBehaviour, IGameManagerAware
    {
        [SerializeField] private TurnOrder turnOrder;
        [SerializeField] public GameObject dmgPrefab;

        private GameObject _playerContainer;
        private GameObject _dmgText;
        public bool isTurn;
        private DiceRoll _diceRoll;
        private CharacterInfo _characterInfo;
        private EnemyController _enemyController;
        private GameManager _gameManager;
        
        public void Initialize(GameManager gameManager)
        {
            _gameManager = gameManager;
            
            // Subscribe to game manager events
            _gameManager.OnCombatStarted += OnCombatStarted;
            _gameManager.OnCombatEnded += OnCombatEnded;
            _gameManager.OnCharacterDamaged += OnCharacterDamaged;
            _gameManager.OnEnemyDamaged += OnEnemyDamaged;
        }
        
        public void OnGameStateChanged(GameManager.GameState newState)
        {
            // React to game state changes
            if (newState == GameManager.GameState.Combat)
            {
                // Combat specific setup
            }
            else if (newState == GameManager.GameState.Exploration)
            {
                // Exploration specific setup
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events when destroyed
            if (_gameManager != null)
            {
                _gameManager.OnCombatStarted -= OnCombatStarted;
                _gameManager.OnCombatEnded -= OnCombatEnded;
                _gameManager.OnCharacterDamaged -= OnCharacterDamaged;
                _gameManager.OnEnemyDamaged -= OnEnemyDamaged;
            }
        }
        
        private void OnCombatStarted()
        {
            Debug.Log("Combat started");
            // Combat initialization logic
        }
        
        private void OnCombatEnded()
        {
            Debug.Log("Combat ended");
            // Combat cleanup logic
        }
        
        private void OnCharacterDamaged(CharacterInfo character, int damage)
        {
            if (character == null) return;
            
            _dmgText = Instantiate(dmgPrefab, character.gameObject.transform);
            _dmgText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(damage.ToString());
        }
        
        private void OnEnemyDamaged(EnemyController enemy, int damage)
        {
            if (enemy == null) return;
            
            _dmgText = Instantiate(dmgPrefab, enemy.gameObject.transform);
            _dmgText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(damage.ToString());
        }
        
        private void Start()
        {
            _playerContainer = GameObject.FindGameObjectWithTag("Players").gameObject;
            if (gameObject.CompareTag("Player"))
            {
                _characterInfo = GetComponentInParent<CharacterInfo>();
            }

            if (gameObject.CompareTag("enemy"))
            {
                _enemyController = GetComponentInParent<EnemyController>();
            }
            
            if (turnOrder == null)
            {
                turnOrder = GameObject.FindGameObjectWithTag("TurnController")?.GetComponent<TurnOrder>();
            }
            
            _diceRoll = new DiceRoll();
            
            // Register with GameManager if not explicitly initialized
            if (_gameManager == null && GameManager.Instance != null)
            {
                Initialize(GameManager.Instance);
            }
        }

        public void StartTurn()
        {
            if (gameObject.CompareTag("Player"))
            {
                _characterInfo.hasAttack = true;
                _characterInfo.hasMovement = true;
                isTurn = true;
                DetectOtherCharacters();
                return;
            }
            _enemyController.hasAttack = true;
            _enemyController.hasMovement = true;
            isTurn = true;
            DetectOtherCharacters();
        }

        public void StopTurn()
        {
            if (gameObject.CompareTag("Player"))
            {
                _characterInfo.hasAttack = false;
                _characterInfo.hasMovement = false;
                isTurn = false;
                foreach (var tile in MapManager.Instance.Map.Values)
                {
                    tile.isBlocked = false;
                }

                turnOrder.startNextTurn = true;
            }
            else if (gameObject.CompareTag("enemy"))
            {
                _enemyController.hasAttack = false;
                _enemyController.hasMovement = false;
                isTurn = false;
                foreach (var tile in MapManager.Instance.Map.Values)
                {
                    tile.isBlocked = false;
                }

                turnOrder.startNextTurn = true;
            }
        }

        private void DetectOtherCharacters()
        {
            if (turnOrder == null || turnOrder.characterList == null) return;
            
            foreach (var character in turnOrder.characterList)
            {
                if (character == null) continue;

                if (character.CompareTag("enemy") && turnOrder.currentInitiative != character.GetComponent<EnemyController>().initiative)
                {
                    var enemyTile = character.GetComponent<EnemyController>().standingOnTile;
                    if (enemyTile != null)
                        enemyTile.isBlocked = true;
                }

                if (character.CompareTag("Player") && turnOrder.currentInitiative != character.GetComponent<CharacterInfo>().initiative)
                {
                    var playerTile = character.GetComponent<CharacterInfo>().standingOnTile;
                    if (playerTile != null)
                        playerTile.isBlocked = true;
                }
            }
        }

        public void AttackOtherCharacter(CharacterInfo info, EnemyController other)
        {
            if (_gameManager != null)
            {
                // Use GameManager for attack resolution
                var hitRoll = _diceRoll.RollToHit(info);
                if (hitRoll < other.armorClass)
                {
                    _dmgText = Instantiate(dmgPrefab, other.transform);
                    _dmgText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText("Miss");
                    return;
                }
                var dmg = _diceRoll.RollDmg(info);
                _gameManager.DamageEnemy(other, dmg);
            }
            else
            {
                // Fallback to legacy behavior
                var hitRoll = _diceRoll.RollToHit(info);
                if (hitRoll < other.armorClass)
                {
                    _dmgText = Instantiate(dmgPrefab, other.transform);
                    _dmgText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText("Miss");
                    return;
                }
                var dmg = _diceRoll.RollDmg(info);
                TakeDamage(dmg, other);
            }
        }
        
        public void AttackOtherCharacter(EnemyController info, CharacterInfo other)
        {
            if (_gameManager != null)
            {
                // Use GameManager for attack resolution
                var hitRoll = _diceRoll.RollToHit(info);
                if (hitRoll < other.armorClass)
                {
                    _dmgText = Instantiate(dmgPrefab, other.transform);
                    _dmgText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText("Miss");
                    return;
                }
                var dmg = _diceRoll.RollDmg(info);
                _gameManager.DamageCharacter(other, dmg);
            }
            else
            {
                // Fallback to legacy behavior
                var hitRoll = _diceRoll.RollToHit(info);
                if (hitRoll < other.armorClass)
                {
                    _dmgText = Instantiate(dmgPrefab, other.transform);
                    _dmgText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText("Miss");
                    return;
                }
                var dmg = _diceRoll.RollDmg(info);
                TakeDamage(dmg, other);
            }
        }

        // Legacy damage handling methods - these will eventually be removed
        // when everything is migrated to GameManager
        private void TakeDamage(int dmg, CharacterInfo other)
        {
            if (dmg >= other.health)
            {
                turnOrder?.characterList.Remove(other.gameObject);
                turnOrder?._sortedInitiatives.Remove(other.initiative);
                turnOrder?._initiatives.Remove(other.initiative);
                gameObject.GetComponent<EnemyController>()?.PlayerDict.Remove(other.gameObject);
                Destroy(other.gameObject);
                Debug.Log("player destroyed");
                if (_playerContainer.transform.childCount == 1)
                {
                    Debug.Log("GameOver");
                    GameOver();
                }
            }
            other.health -= dmg;
            _dmgText = Instantiate(dmgPrefab, other.gameObject.transform);
            _dmgText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(dmg.ToString());
        }
        
        private void TakeDamage(int dmg, EnemyController other)
        {
            if (dmg >= other.health)
            {
                turnOrder?.characterList.Remove(other.gameObject);
                turnOrder?._sortedInitiatives.Remove(other.initiative);
                turnOrder?._initiatives.Remove(other.initiative);
                gameObject.GetComponent<PlayerController>()?.Enemies.Remove(other.gameObject);
                Destroy(other.gameObject);
            }
            other.health -= dmg;
            _dmgText = Instantiate(dmgPrefab, other.gameObject.transform);
            _dmgText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(dmg.ToString());
        }

        private void GameOver()
        {
            if (_gameManager != null)
            {
                _gameManager.EndGame();
            }
            else
            {
                SceneManager.LoadScene("GameOver");
            }
        }
    }
} 