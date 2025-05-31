using System.Collections.Generic;
using Start.Scripts.Character;
using Start.Scripts.Dice;
using Start.Scripts.Enemy;
using Start.Scripts.Game;
using Start.Scripts.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Start.Scripts.Combat
{
    public class CombatController : Component, IManagable
    {
        public bool isTurn;
        private DiceRoll _diceRoll;
        private PlayerController _playerController;
        private EnemyController _enemyController;
        private GameManager _gameManager;

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
            if (gameObject.CompareTag("Player"))
            {
                _playerController = gameObject.GetComponent<PlayerController>();
            }
            else if (gameObject.CompareTag("enemy"))
            {
                _enemyController = gameObject.GetComponent<EnemyController>();
            }
            _diceRoll = new DiceRoll();
        }

        public void StartTurn()
        {
            if (gameObject.CompareTag("Player"))
            {
                _playerController.HasAction = true;
                _playerController.HasMovement = true;
                isTurn = true;
            }
            else if (gameObject.CompareTag("enemy"))
            {
                _enemyController.HasAction = true;
                _enemyController.HasMovement = true;
                isTurn = true;
            }
        }

        public void StopTurn()
        {
            if (gameObject.CompareTag("Player"))
            {
                _playerController.HasAction = false;
                _playerController.HasMovement = false;
                isTurn = false;
                // foreach (var tile in MapManager.Instance.Map.Values)
                // {
                //    tile.isBlocked = false;
                // }
            }
            else if (gameObject.CompareTag("enemy"))
            {
                _enemyController.HasAction = false;
                _enemyController.HasMovement = false;
                isTurn = false;
                // foreach (var tile in MapManager.Instance.Map.Values)
                //{
                //      tile.isBlocked = false;
                // }
            }
        }

        public void AttackOtherCharacter(PlayerController player, EnemyController other)
        {
            if (_gameManager != null)
            {
                // Use GameManager for attack resolution
                var hitRoll = _diceRoll.RollToHit(player);
                if (hitRoll < other.ArmorClass)
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
                if (hitRoll < other.ArmorClass)
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
                _gameManager.Party.RemoveFromParty(other.gameObject);
                Destroy(other.gameObject);
                Debug.Log("player killed");
            }
            other.CurrentHealth -= dmg;
            _gameManager.Combat.DamageCharacter(other, dmg);
        }
        private void TakeDamage(int dmg, EnemyController other)
        {
            if (dmg >= other.CurrentHealth)
            {
                _gameManager.Enemies.RemoveEnemy(other);
                gameObject.GetComponent<PlayerController>()?.Enemies.Remove(other.gameObject);
                Destroy(other.gameObject);
            }
            other.CurrentHealth -= dmg;
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
