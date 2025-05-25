// PartyManager.cs

using System;
using System.Collections.Generic;
using Start.Scripts.Character;
using Start.Scripts.Classes;
using UnityEngine;

namespace Start.Scripts.Game
{
    public class PartyManager : MonoBehaviour
    {
        private readonly List<CharacterInfo> _partyMembers = new();
        private readonly List<GameObject> _partyObjects = new();
        private CharacterInfo _activePlayer;

        public event Action<List<CharacterInfo>> OnPartyUpdated;
        public event Action<CharacterInfo> OnPlayerSpawned;

        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject playerContainer;

        public IReadOnlyList<CharacterInfo> PartyMembers => _partyMembers;
        public CharacterInfo ActivePlayer => _activePlayer;

        public void Initialize()
        {
            _partyMembers.Clear();
            _partyObjects.Clear();
            _activePlayer = null;
            OnPartyUpdated?.Invoke(_partyMembers);
        }

        public CharacterInfo SpawnPlayer(Vector3 position, PlayerClass playerClass = null)
        {
            GameObject playerObject = Instantiate(playerPrefab, position, Quaternion.identity, playerContainer.transform);
            CharacterInfo playerInfo = playerObject.GetComponent<CharacterInfo>();

            if (playerInfo != null)
            {
                if (playerClass != null)
                    playerInfo.playerClass = playerClass;

                _partyObjects.Add(playerObject);
                _partyMembers.Add(playerInfo);

                if (_activePlayer == null)
                    _activePlayer = playerInfo;

                OnPlayerSpawned?.Invoke(playerInfo);
                OnPartyUpdated?.Invoke(_partyMembers);
            }

            return playerInfo;
        }


        public List<CharacterInfo> SpawnPlayers(
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

                var playerGO = Instantiate(playerPrefab, container);
                var info = playerGO.GetComponent<CharacterInfo>();

                if (info != null)
                {
                    info.playerClass = data.playerClass;
                    info.name = data.characterName;
                    info.transform.position = tile.transform.position + new Vector3(0f, 0.0001f, 0f);
                    info.standingOnTile = tile;

                    _partyMembers.Add(info);
                }
            }

            return _partyMembers;
        }

        public void AddToParty(CharacterInfo character)
        {
            if (!_partyMembers.Contains(character))
            {
                _partyMembers.Add(character);
                _partyObjects.Add(character.gameObject);
                OnPartyUpdated?.Invoke(_partyMembers);
            }
        }

        public void RemoveFromParty(CharacterInfo character)
        {
            if (_partyMembers.Contains(character))
            {
                _partyMembers.Remove(character);
                _partyObjects.Remove(character.gameObject);

                if (_activePlayer == character && _partyMembers.Count > 0)
                    _activePlayer = _partyMembers[0];

                OnPartyUpdated?.Invoke(_partyMembers);
            }
        }

        public void SetActivePlayer(CharacterInfo character)
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
