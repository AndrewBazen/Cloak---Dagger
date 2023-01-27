using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Start.Scripts
{
    public class CombatController
    { 
        private List<GameObject> _enemyList; 
        private List<GameObject> _playerList;
        private Dictionary<GameObject, int> _initiatives;

        public void StartCombat()
        {
            GetCharacters();
            if (_playerList.Count > 1)
            {
                GetInitiatives(_playerList);
            }
            GetInitiatives(_enemyList);
        }

        private void GetCharacters()
        {
            _playerList = GameObject.FindGameObjectsWithTag("Player").ToList();
            _enemyList = GameObject.FindGameObjectsWithTag("enemy").ToList();
        }

        private void GetInitiatives(List<GameObject> list)
        {
            foreach (var character in list)
            {
                if (character.GetComponent<EnemyData>() != null)
                {
                    _initiatives.Add(character, character.GetComponent<EnemyData>().initiative);
                }
                else
                {
                    _initiatives.Add(character, character.GetComponent<CharacterInfo>().initiative);
                }

            }
        }

        private void DetermineTurnOrder()
        {
            
        }
    }
}
