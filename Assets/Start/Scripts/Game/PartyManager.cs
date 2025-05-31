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

namespace Start.Scripts.Game
{
    public class PartyManager : MonoBehaviour
    {
        private readonly List<CharacterInfoData> _partyInfo = new();
        private readonly List<GameObject> _partyObjects = new();
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


        public void Initialize()
        {
            _partyInfo.Clear();
            _partyObjects.Clear();
            _activePlayer = null;
            _party.Clear();
            _currentLevelData = null;
            ActorRegistry.OnActorRegistered += HandleRegister;
            ActorRegistry.OnActorUnregistered += HandleUnregister;
            OnPartyUpdated?.Invoke(_partyInfo);
        }

        private void HandleRegister(Actor actor)
        {
            if (actor is PlayerController playerController)
            {
                _party.Add(playerController);
                _partyObjects.Add(playerController.gameObject);
                _partyInfo.Add(playerController.characterData);
                OnPartyUpdated?.Invoke(_partyInfo);
            }
        }
        private void HandleUnregister(Actor actor)
        {
            if (actor is PlayerController playerController)
            {
                _party.Remove(playerController);
                _partyObjects.Remove(playerController.gameObject);
                _partyInfo.Remove(playerController.characterData);
                OnPartyUpdated?.Invoke(_partyInfo);
            }
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

        //TODO: create a LevelData Lodar that handles loading level data from a file or database
        public List<CharacterInfoData> SpawnPlayers(
            List<CharacterLoadData> partyData,
            List<OverlayTile> spawnTiles,
            GameObject playerPrefab,
            Transform container)
        {
            _partyInfo.Clear();

            for (int i = 0; i < partyData.Count && i < spawnTiles.Count; i++)
            {
                var data = partyData[i];
                var tile = spawnTiles[i];

                if (data == null || tile == null)
                    continue;

                if (data.playerClass == null)
                {
                    Debug.LogWarning($"Player class is null for character {data.characterName}. Skipping spawn.");
                    continue;
                }

                SpawnPlayer(tile.transform.position, data.playerClass);

                if (info != null)
                {
                    info.playerClass = data.playerClass;
                    info.name = data.characterName;
                    playerGO.transform.position = tile.transform.position + new Vector3(0f, 0.0001f, 0f);

                    _partyInfo.Add(info);
                }
            }

            return _partyInfo;
        }

        public void AddToParty(GameObject character)
        {
            var c = character.GetComponent<PlayerController>();
            if (!_partyInfo.Contains(c.characterData))
            {
                _partyInfo.Add(c.characterData);
                _partyObjects.Add(character);
                _party.Add(c);
                OnPartyUpdated?.Invoke(_partyInfo);
            }
        }

        public void RemoveFromParty(GameObject character)
        {
            var c = character.GetComponent<PlayerController>();
            if (_partyInfo.Contains(c.characterData))
            {
                _partyInfo.Remove(c.characterData);
                _partyObjects.Remove(character);
                _party.Remove(c);
                if (_activePlayer == character && _partyInfo.Count > 0)
                    _activePlayer = _partyInfo[0];

                OnPartyUpdated?.Invoke(_partyInfo);
            }
        }

        public void SetActivePlayer(CharacterInfoData character)
        {
            if (_partyInfo.Contains(character))
                _activePlayer = character;
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
