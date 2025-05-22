using System.Collections.Generic;
using Start.Scripts.Character;
using Start.Scripts.Enemy;
using Start.Scripts.Game;
using UnityEngine;

namespace Start.Scripts.Combat
{
    public class TurnOrder : MonoBehaviour
    {
        [SerializeField] private GameObject playerContainer;
        [SerializeField] private GameObject enemyContainer;
        public int currentInitiative;
        public List<GameObject> characterList;
        public List<int> _initiatives;
        public List<int> _sortedInitiatives;
        private GameObject _character;
        public bool startNextTurn;
        private int _index;
        private int _tempIndex;
        private Dictionary<int, GameObject> _initiativeDict;
        
        private void Start()
        {
            // Find references if needed
            if (playerContainer == null)
                playerContainer = GameObject.FindGameObjectWithTag("Players");
            if (enemyContainer == null)
                enemyContainer = GameObject.FindGameObjectWithTag("Enemies");
                
            // Initialize data structures
            _initiatives = new List<int>();
            _sortedInitiatives = new List<int>();
            characterList = new List<GameObject>();
            _initiativeDict = new Dictionary<int, GameObject>();
            startNextTurn = false;
        }

        /// <summary>
        /// Initialize the turn order system with characters and enemies from the GameManager
        /// </summary>
        public void InitializeTurnOrder(List<CharacterInfo> partyMembers, List<EnemyController> enemies)
        {
            // Clear previous data
            _initiatives.Clear();
            _sortedInitiatives.Clear();
            characterList.Clear();
            _initiativeDict.Clear();
            startNextTurn = false;
            
            // Add all player characters to the list
            foreach (var player in partyMembers)
            {
                if (player == null || player.gameObject == null)
                    continue;
                    
                characterList.Add(player.gameObject);
                var playerInitiative = player.initiative;
                
                // If two characters have the same initiative, add a small offset
                while (_initiatives.Contains(playerInitiative))
                {
                    playerInitiative += 1;
                }
                
                _initiatives.Add(playerInitiative);
                _sortedInitiatives.Add(playerInitiative);
                _initiativeDict[playerInitiative] = player.gameObject;
            }
            
            // Add all enemies to the list
            foreach (var enemy in enemies)
            {
                if (enemy == null || enemy.gameObject == null)
                    continue;
                    
                characterList.Add(enemy.gameObject);
                var enemyInitiative = enemy.initiative;
                
                // If two characters have the same initiative, add a small offset
                while (_initiatives.Contains(enemyInitiative))
                {
                    enemyInitiative += 1;
                }
                
                _initiatives.Add(enemyInitiative);
                _sortedInitiatives.Add(enemyInitiative);
                _initiativeDict[enemyInitiative] = enemy.gameObject;
            }
            
            // Sort the initiatives in descending order
            _sortedInitiatives.Sort();
            _sortedInitiatives.Reverse();
            
            // If no characters, don't proceed
            if (_sortedInitiatives.Count == 0)
                return;
                
            // Start the first turn
            currentInitiative = _sortedInitiatives[0];
            _index = 0;
            _tempIndex = 0;
            _character = _initiativeDict[currentInitiative];
            
            // Begin first turn
            GameEvents.current.OnTurnStart?.Invoke(StartTurn, GetInRangeTiles);
        }

        // This is the legacy method that reads directly from containers
        // Use only for backward compatibility
        private void InitializeTurnOrderFromContainers()
        {
            _initiatives = new List<int>();
            _sortedInitiatives = new List<int>();
            characterList = new List<GameObject>();
            _initiativeDict = new Dictionary<int, GameObject>();
            startNextTurn = false;
            
            // Add all player characters to the list
            for (var i = 0; i < playerContainer.transform.childCount; i++)
            {
                var player = playerContainer.transform.GetChild(i).gameObject;
                characterList.Add(player);
                var playerInitiative = player.GetComponent<CharacterInfo>().initiative;
                
                // If two characters have the same initiative, add a small offset
                while (_initiatives.Contains(playerInitiative))
                {
                    playerInitiative += 1;
                }
                
                _initiatives.Add(playerInitiative);
                _sortedInitiatives.Add(playerInitiative);
                _initiativeDict[playerInitiative] = player;
            }
            
            // Add all enemies to the list
            for (var i = 0; i < enemyContainer.transform.childCount; i++)
            {
                var enemy = enemyContainer.transform.GetChild(i).gameObject;
                characterList.Add(enemy);
                var enemyInitiative = enemy.GetComponent<EnemyController>().initiative;
                
                // If two characters have the same initiative, add a small offset
                while (_initiatives.Contains(enemyInitiative))
                {
                    enemyInitiative += 1;
                }
                
                _initiatives.Add(enemyInitiative);
                _sortedInitiatives.Add(enemyInitiative);
                _initiativeDict[enemyInitiative] = enemy;
            }
            
            // Sort the initiatives in descending order
            _sortedInitiatives.Sort();
            _sortedInitiatives.Reverse();
            
            // Start the first turn
            currentInitiative = _sortedInitiatives[0];
            _index = 0;
            _tempIndex = 0;
            _character = _initiativeDict[currentInitiative];
            
            // Begin first turn
            GameEvents.current.OnTurnStart?.Invoke(StartTurn, GetInRangeTiles);
        }

        private void Update()
        {
            // Check if the next turn should start
            if (!startNextTurn) return;
            
            // Reset the flag
            startNextTurn = false;
            
            // If no initiatives, don't proceed
            if (_sortedInitiatives.Count == 0)
                return;
                
            // Move to the next initiative in the sorted list
            _index = (_index + 1) % _sortedInitiatives.Count;
            currentInitiative = _sortedInitiatives[_index];
            
            // Make sure the character still exists
            if (!_initiativeDict.ContainsKey(currentInitiative))
            {
                // Character was removed, skip to next
                startNextTurn = true;
                return;
            }
            
            _character = _initiativeDict[currentInitiative];
            
            // Notify GameManager about turn change
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnTurnChanged?.Invoke(currentInitiative);
            }
            
            // Start the new turn
            GameEvents.current.OnTurnStart?.Invoke(StartTurn, GetInRangeTiles);
        }
        
        public void RemoveCharacterFromTurnOrder(GameObject character)
        {
            if (characterList.Contains(character))
            {
                characterList.Remove(character);
                
                // Find and remove initiative entry
                int initiativeToRemove = -1;
                foreach (var entry in _initiativeDict)
                {
                    if (entry.Value == character)
                    {
                        initiativeToRemove = entry.Key;
                        break;
                    }
                }
                
                if (initiativeToRemove != -1)
                {
                    _initiatives.Remove(initiativeToRemove);
                    _sortedInitiatives.Remove(initiativeToRemove);
                    _initiativeDict.Remove(initiativeToRemove);
                }
            }
        }

        private void StartTurn()
        {
            // This is a placeholder that will be overridden by the event subscriber
        }

        private void GetInRangeTiles()
        {
            // This is a placeholder that will be overridden by the event subscriber
        }
    }
} 