// PartyManager.cs

using System;
using System.Collections.Generic;
using Start.Scripts.Character;
using Start.Scripts.Classes;
using Start.Scripts.Inventory;
using UnityEngine;
using Start.Scripts.Level;

namespace Start.Scripts.Game
{
    public class PartyManager : MonoBehaviour
    {
        private readonly List<CharacterInfoData> _partyMembers = new();
        private readonly List<GameObject> _partyObjects = new();
        private readonly List<PlayerController> _partyControllers = new();
        private CharacterInfoData _activePlayer;
        private LevelData _currentLevelData;

        public event Action<List<CharacterInfoData>> OnPartyUpdated;
        public event Action<CharacterInfoData> OnPlayerSpawned;
        public event Action<List<int>> OnCharacterStatsChanged;
        public event Action<InventoryHolder> OnCharacterInventoryChanged;

        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject playerContainer;

        public IReadOnlyList<CharacterInfoData> PartyMembers => _partyMembers;
        public IReadOnlyList<GameObject> PartyObjects => _partyObjects;
        public IReadOnlyList<PlayerController> PartyControllers => _partyControllers;
        public CharacterInfoData ActivePlayer => _activePlayer;
        public LevelData CurrentLevelData => _currentLevelData;

        public void Initialize()
        {
            _partyMembers.Clear();
            _partyObjects.Clear();
            _activePlayer = null;
            _partyControllers.Clear();
            _currentLevelData = null;
            OnPartyUpdated?.Invoke(_partyMembers);
        }

        public CharacterInfoData SpawnPlayer(Vector3 position, PlayerClass playerClass = null)
        {
            GameObject playerObject = Instantiate(playerPrefab, position, Quaternion.identity, playerContainer.transform);
            var playerData = playerObject.GetComponent<PlayerController>().characterData;

            if (playerData != null)
            {
                if (playerClass != null)
                    playerData.playerClass = playerClass;

                _partyObjects.Add(playerObject);
                _partyMembers.Add(playerData);

                if (_activePlayer == null)
                    _activePlayer = playerData;

                OnPlayerSpawned?.Invoke(playerData);
                OnPartyUpdated?.Invoke(_partyMembers);
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
            _partyMembers.Clear();

            for (int i = 0; i < partyData.Count && i < spawnTiles.Count; i++)
            {
                var data = partyData[i];
                var tile = spawnTiles[i];

                var player = Instantiate(playerPrefab, container);
                var playerGO = player.GetComponent<PlayerController>();
                var info = player.GetComponent<PlayerController>().characterData;

                if (info != null)
                {
                    info.playerClass = data.playerClass;
                    info.name = data.characterName;
                    playerGO.transform.position = tile.transform.position + new Vector3(0f, 0.0001f, 0f);

                    _partyMembers.Add(info);
                }
            }

            return _partyMembers;
        }

        public void AddToParty(GameObject character)
        {
            var c = character.GetComponent<PlayerController>();
            if (!_partyMembers.Contains(c.characterData))
            {
                _partyMembers.Add(c.characterData);
                _partyObjects.Add(character);
                OnPartyUpdated?.Invoke(_partyMembers);
            }
        }

        public void RemoveFromParty(GameObject character)
        {
            var c = character.GetComponent<PlayerController>();
            if (_partyMembers.Contains(c.characterData))
            {
                _partyMembers.Remove(c.characterData);
                _partyObjects.Remove(character);

                if (_activePlayer == character && _partyMembers.Count > 0)
                    _activePlayer = _partyMembers[0];

                OnPartyUpdated?.Invoke(_partyMembers);
            }
        }

        public void SetActivePlayer(CharacterInfoData character)
        {
            if (_partyMembers.Contains(character))
                _activePlayer = character;
        }

        public void ClearParty()
        {
            foreach (var playerObject in _partyObjects)
            {
                if (playerObject != null)
                    Destroy(playerObject);
            }

            _partyMembers.Clear();
            _partyObjects.Clear();
            _activePlayer = null;
            OnPartyUpdated?.Invoke(_partyMembers);
        }
    }
}
