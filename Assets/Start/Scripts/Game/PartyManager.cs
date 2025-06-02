// PartyManager.cs

using System;
using System.Collections.Generic;
using Start.Scripts.Character;
using Start.Scripts.Classes;
using Start.Scripts.Inventory;
using Start.Resources;
using Start.Scripts.BaseClasses;
using UnityEngine;
using Start.Scripts.Levels;
using Unity.VisualScripting;

namespace Start.Scripts.Game
{
    public class PartyManager : MonoBehaviour
    {
        public static PartyManager Instance { get; private set; }
        private List<CharacterInfoData> _partyInfo;
        private List<GameObject> _partyObjects;
        private List<PlayerController> _party;
        private CharacterInfoData _activePlayer;
        private LevelData _currentLevelData;

        public event Action<List<CharacterInfoData>> OnPartyUpdated;
        public event Action<CharacterInfoData> OnPlayerSpawned;
        public event Action<List<int>> OnCharacterStatsChanged;
        public event Action<InventoryHolder> OnCharacterInventoryChanged;
        public event Action OnPlayerMoved;

        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject playerContainer;

        public List<CharacterInfoData> PartyMembers => _partyInfo;
        public List<GameObject> PartyObjects => _partyObjects;
        public CharacterInfoData ActivePlayer => _activePlayer;
        public LevelData CurrentLevelData => _currentLevelData;
        public List<PlayerController> Party
        {
            get => _party;
            set
            {
                _party = value;
                OnPartyUpdated?.Invoke(_partyInfo);
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }


        public void Initialize()
        {
            _partyInfo = new List<CharacterInfoData>();
            _partyObjects = new List<GameObject>();
            _activePlayer = null;
            _party = new List<PlayerController>();
            _currentLevelData = null;
            OnPartyUpdated?.Invoke(_partyInfo);
        }

        public void UpdatePartyData()
        {
            _partyInfo.Clear();
            foreach (var player in _party)
            {
                _partyInfo.Add(player.characterData);
            }
            OnPartyUpdated?.Invoke(_partyInfo);
        }

        public PlayerController GetPlayer(int id)
        {
            return _party.Find(p => p.characterData.Id == id);
        }

        public CharacterInfoData SpawnPlayer(Vector3 position, PlayerClass playerClass = null)
        {
            GameObject playerObject = Instantiate(playerPrefab, position, Quaternion.identity, playerContainer.transform);
            var playerData = playerObject.GetComponent<PlayerController>().characterData;

            if (playerData != null)
            {
                if (playerClass != null)
                    playerData.PlayerClass = playerClass;

                _partyObjects.Add(playerObject);
                _partyInfo.Add(playerData);

                if (_activePlayer == null)
                    _activePlayer = playerData;

                OnPlayerSpawned?.Invoke(playerData);
                OnPartyUpdated?.Invoke(_partyInfo);
            }

            return playerData;
        }

        //TODO: create a LevelData Loader that handles loading level data from a file or database

        public void SpawnPlayers(List<CharacterInfoData> partyData)
        {
            for (int i = 0; i < partyData.Count; i++)
            {
                var data = partyData[i];
                GameObject playerObject = Instantiate(playerPrefab, playerContainer.transform);
                var playerController = playerObject.GetComponent<PlayerController>();
                playerController.characterData = data;
                _party.Add(playerController);
                _partyObjects.Add(playerObject);
                _partyInfo.Add(data);
            }
        }

        public List<CharacterInfoData> SpawnPlayers(
            List<CharacterInfoData> partyData,
            List<OverlayTile> spawnTiles)
        {
            _partyInfo.Clear();

            for (int i = 0; i < partyData.Count && i < spawnTiles.Count; i++)
            {
                var data = partyData[i];
                var tile = spawnTiles[i];

                if (data == null || tile == null)
                    continue;

                if (data.PlayerClass == null)
                {
                    Debug.LogWarning($"Player class is null for character {data.Id}. Skipping spawn.");
                    continue;
                }

                var playerData = SpawnPlayer(tile.transform.position, data.PlayerClass);
                var playerController = playerData.GetComponent<PlayerController>();
                var playerGO = playerController.gameObject;


                if (playerData != null)
                {
                   
                    if (tile != null)
                    {
                        playerData.TilePos = tile;
                        playerGO.transform.position = tile.transform.position + new Vector3(0f, 0.0001f, 0f);
                    }

                    _partyInfo.Add(playerData);
                }
            }

            return _partyInfo;
        }

        public void AddToParty(CharacterInfoData character)
        {
            if (!_partyInfo.Contains(character))
            {
                _partyInfo.Add(character);
                OnPartyUpdated?.Invoke(_partyInfo);
            }
        }

        public void RemoveFromParty(CharacterInfoData character)
        {
            if (_partyInfo.Contains(character))
            {
                _partyInfo.Remove(character);
                if (_activePlayer == character && _partyInfo.Count > 0)
                    _activePlayer = _partyInfo[0];

                OnPartyUpdated?.Invoke(_partyInfo);
            }
        }

        public void SetActivePlayer(CharacterInfoData character)
        {
            if (_partyInfo.Contains(character))
                _activePlayer = character;
            OnPartyUpdated?.Invoke(_partyInfo);
        }

        public void UpdateParty(List<CharacterInfoData> partyData)
        {
            _partyInfo = partyData;
            OnPartyUpdated?.Invoke(_partyInfo);
        }

        public void ClearParty()
        {
            foreach (var playerObject in _partyObjects)
            {
                if (playerObject != null)
                    Destroy(playerObject);
            }

            _partyInfo.Clear();
            _partyObjects.Clear();
            _activePlayer = null;
            OnPartyUpdated?.Invoke(_partyInfo);
        }
    }
}
