using System;
using System.Collections.Generic;
using Start.Scripts.Enemy;
using Start.Scripts.Serialization;
using Start.Scripts.Services;
using UnityEngine;

namespace Start.Scripts.Game
{
    public class GameManager : MonoBehaviour {
        private static GameManager _instance;
        public static GameManager Instance => _instance;

        global::System.Random _random = new global::System.Random();
        [SerializeField] public GameObject PlayerPrefab;
        [SerializeField] public GameObject EnemyPrefab;
        [SerializeField] public GameObject PlayerContainer;
        [SerializeField] public GameObject EnemyContainer;
        [SerializeField] public GameObject Camera;
        [SerializeField] public GameObject Mapmanger;

        private SaveSystem _saveSystem = new SaveSystem();
        private SpawnService _spawnService = new SpawnService();
        private CombatController _combatController = new CombatController();
        private GameEvents _gameEvents = new GameEvents();


        // Add this field to hold enemy GameObjects
        [SerializeField] private List<GameObject> _enemies = new List<GameObject>();
        // Add this field to hold party GameObjects
        [SerializeField] private List<GameObject> _party = new List<GameObject>();

        public List<EnemyData> Enemies {
            get {
                var enemyDataList = new List<EnemyData>();
                for (int i = 0; i < _enemies.Count; i++) {
                    var controller = _enemies[i].GetComponent<EnemyController>();
                    if (controller != null && controller.data != null) {
                        enemyDataList.Add(controller.data);
                    }
                }
                return enemyDataList;
            }
        }

        public List<CharacterInfo> Party {
            get {
                var partyDataList = new List<CharacterInfo>();
                for (int i = 0; i < _party.Count; i++) {
                    var controller = _party[i].GetComponent<CharacterInfo>();
                    if (controller != null && controller.data != null) {
                        partyDataList.Add(controller.data);
                    }
                }
                return partyDataList;
            }
        }



        public int level;
   

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            } else
            {
                _instance = this;
            }
            
            _saveSystem = new SaveSystem();
            party = new List<CharacterInfo>();
            
        }

        public void SaveParty()
        {
            var saveName = DateTime.Now.ToLongDateString() + DateTime.Now.ToLongTimeString();
            _saveSystem.SaveGameData(saveName, party, enemies);
        }
    }
}