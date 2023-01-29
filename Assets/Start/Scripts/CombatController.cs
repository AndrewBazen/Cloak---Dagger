using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Start.Scripts
{
    public class CombatController : MonoBehaviour
    {
        private IOrderedEnumerable<KeyValuePair<int, GameObject>> _currentTurnOrder;
        private List<GameObject> _characterList;
        private List<GameObject> _enemyList; 
        private List<GameObject> _playerList;
        private Dictionary<int, GameObject> _initiatives;
        private List<int> sortedInitiatives;
        private int turnCount;

        private void Start()
        {
            _currentTurnOrder = null;
            _characterList = new List<GameObject>();
            _enemyList = new List<GameObject>();
            _playerList = new List<GameObject>();
            _initiatives = new Dictionary<int, GameObject>();
            sortedInitiatives = new List<int>();
            turnCount = 0;
        }

        public void StartTurn()
        {
            turnCount = 0;
            GetCharacters();
            GetInitiatives(_characterList);
            GetTurnOrder();
            
            if (!_initiatives[sortedInitiatives[turnCount]].CompareTag("Player"))
            {
                _initiatives[sortedInitiatives[turnCount]].GetComponent<EnemyController>().isEnemysTurn = true;
                DetectOtherCharacters();
                return;
            }

            _initiatives[sortedInitiatives[turnCount]].GetComponent<PlayerController>().isPlayersTurn = true;
            DetectOtherCharacters();
            
        }

        public void StopTurn()
        {
            foreach (var tile in MapManager.Instance.Map.Values)
            {
               tile.isBlocked = false;
            }

            if (!_initiatives[sortedInitiatives[turnCount]].CompareTag("Player"))
            {
                _initiatives[sortedInitiatives[turnCount]].GetComponent<EnemyController>().isEnemysTurn = false;
                turnCount++;
                return;
            }

            _initiatives[sortedInitiatives[turnCount]].GetComponent<PlayerController>().isPlayersTurn = false;
            turnCount++;
            Debug.Log(turnCount);


        }

        private void DetectOtherCharacters()
        {
            foreach (var character in _characterList)
            {
                if (character.CompareTag("enemy"))
                {
                    if (!character.GetComponent<EnemyController>().isEnemysTurn)
                    {
                        character.GetComponent<CharacterInfo>().standingOnTile.isBlocked = true;
                    }
                    continue;
                }
                if (!character.GetComponent<PlayerController>().isPlayersTurn)
                {
                    character.GetComponent<CharacterInfo>().standingOnTile.isBlocked = true;
                }
            }
        }

        private void GetCharacters()
        {
            _playerList = GameObject.FindGameObjectsWithTag("Player").ToList();
            _enemyList = GameObject.FindGameObjectsWithTag("enemy").ToList();
            foreach (var player in _playerList)
            {
                if (!_characterList.Contains(player))
                {
                    _characterList.Add(player);
                }
            }
            foreach (var enemy in _enemyList)
            {
                if (!_characterList.Contains(enemy))
                {
                    _characterList.Add(enemy);
                }
            }
           
        }

        private void GetInitiatives(List<GameObject> list)
        {
            foreach (var character in list)
            {
                if (character.CompareTag("enemy"))
                {
                    if (_initiatives.ContainsKey(character.gameObject.GetComponent<EnemyController>().initiative))
                    {
                        var init = character.gameObject.GetComponent<EnemyController>().initiative;
                        while (_initiatives.ContainsKey(init))
                        {
                            init++;
                        }
                        _initiatives.Add(init, character);
                    } else
                    {
                        _initiatives.Add(character.gameObject.GetComponent<EnemyController>().initiative, character);
                    }

                }
                else 
                {
                    if (_initiatives.ContainsKey(character.gameObject.GetComponent<CharacterInfo>().initiative))
                    {
                        var init = character.gameObject.GetComponent<CharacterInfo>().initiative;
                        while (_initiatives.ContainsKey(init))
                        {
                            init++;
                        }
                        _initiatives.Add(init, character);
                    } else
                    {
                        _initiatives.Add(character.gameObject.GetComponent<CharacterInfo>().initiative, character);
                    }
                    
                }

            }
        }

        private void GetTurnOrder()
        {
            sortedInitiatives= _initiatives.Keys.ToList();
            for (int i = 0; i < sortedInitiatives.Count; i++)
            {
                var val = sortedInitiatives[i];
                var flag = 0;
                for (var j = i - 1; j >= 0 && flag != 1;)
                {
                    if (val < sortedInitiatives[j])
                    {
                        sortedInitiatives[j + 1] = sortedInitiatives[j];
                        j--;
                        sortedInitiatives[j + 1] = val;
                    }
                    else flag = 1;
                }
            }
            sortedInitiatives.Reverse();

        }
    }
}
