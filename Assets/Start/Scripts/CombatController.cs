using System.Collections.Generic;
using Start.Scripts.Dice;
using Start.Scripts.Enemy;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vector2 = System.Numerics.Vector2;


namespace Start.Scripts
{
    public class CombatController : MonoBehaviour
    {
        [SerializeField] private TurnOrder turnOrder;
        [SerializeField] public GameObject dmgPrefab;

        private GameObject _playerContainer;
        private GameObject _dmgText;
        public bool isTurn;
        private DiceRoll _diceRoll;
        private CharacterInfo _characterInfo;
        private EnemyController _enemyController;
        
        
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
            turnOrder = GameObject.FindGameObjectWithTag("TurnController").GetComponent<TurnOrder>();
            _diceRoll = new DiceRoll();
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
            if (!gameObject.CompareTag("enemy")) return;
            _enemyController.hasAttack = false;
            _enemyController.hasMovement = false;
            isTurn = false;
            foreach (var tile in MapManager.Instance.Map.Values)
            {
                tile.isBlocked = false;
            }

            turnOrder.startNextTurn = true;

        }

        private void DetectOtherCharacters()
        {
            foreach (var character in turnOrder.characterList)
            {

                if (character.CompareTag("enemy") && turnOrder.currentInitiative != character.GetComponent<EnemyController>().initiative)
                {
                    character.GetComponent<EnemyController>().standingOnTile.isBlocked = true;
                }

                if (character.CompareTag("Player") && turnOrder.currentInitiative != character.GetComponent<CharacterInfo>().initiative)
                {
                    character.GetComponent<CharacterInfo>().standingOnTile.isBlocked = true;
                }
            }
        }

        public void AttackOtherCharacter(CharacterInfo info, EnemyController other)
        {
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
        
        public void AttackOtherCharacter(EnemyController info, CharacterInfo other)
        {
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

        private void TakeDamage(int dmg, CharacterInfo other)
        {
            if (dmg >= other.health)
            {
                turnOrder.characterList.Remove(other.gameObject);
                turnOrder._sortedInitiatives.Remove(other.initiative);
                turnOrder._initiatives.Remove(other.initiative);
                gameObject.GetComponent<EnemyController>().PlayerDict.Remove(other.gameObject);
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
                turnOrder.characterList.Remove(other.gameObject);
                turnOrder._sortedInitiatives.Remove(other.initiative);
                turnOrder._initiatives.Remove(other.initiative);
                gameObject.GetComponent<PlayerController>().enemies.Remove(other.gameObject);
                Destroy(other.gameObject);
            }
            other.health -= dmg;
            _dmgText = Instantiate(dmgPrefab, other.gameObject.transform);
            _dmgText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(dmg.ToString());
        }

        private void GameOver()
        {
            SceneManager.LoadScene("GameOver");
        }

    }
}
