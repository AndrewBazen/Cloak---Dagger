using System;
using System.Collections.Generic;
using System.Linq;
using Start.Scripts.Enemy;
using SuperTiled2Unity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Start.Scripts
{
    public class TurnOrder : MonoBehaviour
    {
        public bool startNextTurn;
        public int currentInitiative;
        public List<GameObject> characterList;
        private List<GameObject> _enemyList; 
        private List<GameObject> _playerList;
        public Dictionary<int, GameObject> _initiatives;
        public List<int> _sortedInitiatives;
        private const float Delay = .1f;
        private float timer;


        private void Start()
        {
            timer = 0f;
            characterList = new List<GameObject>();
            _enemyList = new List<GameObject>();
            _playerList = new List<GameObject>();
            _initiatives = new Dictionary<int, GameObject>();
            _sortedInitiatives = new List<int>();
            startNextTurn = true;
            
        }

        private void Update()
        {
            if (!_initiatives.IsEmpty())
            {
                if (startNextTurn)
                {
                    timer += Time.deltaTime;
                    if (timer > Delay)
                    {
                        currentInitiative = _sortedInitiatives[0];
                        _initiatives[currentInitiative].GetComponent<CombatController>().StartTurn();
                        _sortedInitiatives.RemoveAt(0);
                        _sortedInitiatives.Add(currentInitiative);
                        startNextTurn = false;
                        timer = 0f;
                    }
                    
                }
            }
            else
            {
                GetInitiatives();
                GetTurnOrder();
            }
        }
        
        private void GetCharacters()
        {
           _playerList = GameObject.FindGameObjectsWithTag("Player").ToList();
           _enemyList = GameObject.FindGameObjectsWithTag("enemy").ToList();
           characterList.AddRange(_playerList);
           characterList.AddRange(_enemyList);
        }
        
        private void GetTurnOrder()
        {
            _sortedInitiatives = _initiatives.Keys.ToList();
            _sortedInitiatives.Sort();
            _sortedInitiatives.Reverse();
        }
        
        public void EndTurn()
        {
            if (_initiatives[currentInitiative].CompareTag("Player"))
                _initiatives[currentInitiative].GetComponent<CombatController>().StopTurn();
        }

        private void GetInitiatives()
        {
            GetCharacters();
            foreach (var character in characterList)
            {
                if (character.CompareTag("Player"))
                {
                    if (_initiatives.ContainsKey(character.GetComponent<CharacterInfo>().initiative))
                    {
                        var init = character.GetComponent<CharacterInfo>().initiative;
                        while (_initiatives.ContainsKey(init))
                        {
                            init++;
                        }
                        _initiatives.Add(init, character);
                        character.GetComponent<CharacterInfo>().initiative = init;
                    } else
                    {
                        _initiatives.Add(character.GetComponent<CharacterInfo>().initiative, character);
                    }
                    continue;
                }
                if (_initiatives.ContainsKey(character.GetComponent<EnemyController>().initiative))
                {
                    var init = character.GetComponent<EnemyController>().initiative;
                    while (_initiatives.ContainsKey(init))
                    {
                        init++;
                    }
                    _initiatives.Add(init, character);
                    character.GetComponent<EnemyController>().initiative = init;
                } else
                {
                    _initiatives.Add(character.GetComponent<EnemyController>().initiative, character);
                }
            }
        }
    }
}