using System.Collections.Generic;
using Start.Scripts.Character;
using Start.Scripts.Dice;
using Start.Scripts.Enemy;
using Start.Scripts.Game;
using Start.Scripts.Map;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        private CharacterInfoData _characterData;
        private EnemyController _enemyController;
        private GameManager _gameManager;
        public void Initialize(GameManager gameManager)
        {
            _gameManager = gameManager;
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
        private void Start()
        {
            _playerContainer = GameObject.FindGameObjectWithTag("Players").gameObject;
            if (gameObject.CompareTag("Player"))
            {
                _characterData = GetComponentInParent<CharacterInfoData>();
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
                _characterData.hasAttack = true;
                _characterData.hasMovement = true;
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
                _characterData.hasAttack = false;
                _characterData.hasMovement = false;
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

                if (character.CompareTag("Player") && turnOrder.currentInitiative != character.GetComponent<PlayerController>().Initiative)
                {
                    var playerTile = character.GetComponent<PlayerController>().StandingOnTile;
                    if (playerTile != null)
                        playerTile.isBlocked = true;
                }
            }
        }

        public void AttackOtherCharacter(PlayerController player, EnemyController other)
        {
            if (_gameManager != null)
            {
                // Use GameManager for attack resolution
                var hitRoll = _diceRoll.RollToHit(player);
                if (hitRoll < other.armorClass)
                {
                    _gameManager.Combat.DamageEnemy(other, 0); // No damage on miss
                    return;
                }
                var dmg = _diceRoll.RollDmg(player);
                _gameManager.Combat.DamageEnemy(other, dmg);
            }
            else
            {
                // Fallback to legacy behavior
                var hitRoll = _diceRoll.RollToHit(player);
                if (hitRoll < other.armorClass)
                {
                    _gameManager.Combat.DamageEnemy(other, 0); // No damage on miss
                    return;
                }
                var dmg = _diceRoll.RollDmg(player);
                TakeDamage(dmg, other);
            }
        }
        public void AttackOtherCharacter(EnemyController info, PlayerController other)
        {
            if (_gameManager != null)
            {
                // Use GameManager for attack resolution
                var hitRoll = _diceRoll.RollToHit(info);
                if (hitRoll < other.characterData.armorClass)
                {
                    _gameManager.Combat.DamageCharacter(other, 0); // No damage on miss
                    return;
                }
                var dmg = _diceRoll.RollDmg(info);
                _gameManager.Combat.DamageCharacter(other, dmg);
            }
            else
            {
                // Fallback to legacy behavior
                var hitRoll = _diceRoll.RollToHit(info);
                if (hitRoll < other.characterData.armorClass)
                {
                    _gameManager.Combat.DamageCharacter(other, 0); // No damage on miss
                    return;
                }
                var dmg = _diceRoll.RollDmg(info);
                TakeDamage(dmg, other);
            }
        }

        // Legacy damage handling methods - these will eventually be removed
        // when everything is migrated to GameManager
        private void TakeDamage(int dmg, PlayerController other)
        {
            if (dmg >= other.CurrentHealth)
            {
                turnOrder?.characterList.Remove(other.gameObject);
                turnOrder?._sortedInitiatives.Remove(other.Initiative);
                turnOrder?._initiatives.Remove(other.Initiative);
                GameManager.Instance.Party.RemoveFromParty(other.gameObject);
                Destroy(other.gameObject);
                Debug.Log("player killed");
                if (_playerContainer.transform.childCount == 1)
                {
                    Debug.Log("GameOver");
                    GameOver();
                }
            }
            other.CurrentHealth -= dmg;
            _gameManager.Combat.DamageCharacter(other, dmg);
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
            _gameManager.Combat.DamageEnemy(other, dmg);
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
