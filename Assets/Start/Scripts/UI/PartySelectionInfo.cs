using UnityEngine;
using UnityEngine.UI;
using Start.Scripts.Classes;
using Start.Scripts.Character;
using Start.Scripts.Game;
using System.Collections.Generic;
using Start.Scripts.UI;
using System.Linq;

public class PartySelectionInfo : MonoBehaviour
{
    public List<CharacterInfoData> _characterInfoData;
    public List<PlayerClassInfo> _playerClassInfos;
    public GameManager _gameManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _gameManager = GameManager.Instance;
        _characterInfoData = new List<CharacterInfoData>();
        _playerClassInfos = new List<PlayerClassInfo>();
    }

    public void SetupParty()
    {
        _playerClassInfos = GetComponentsInChildren<PlayerClassInfo>().ToList();
        foreach (var playerClassInfo in _playerClassInfos)
        {
            _playerClassInfos.Add(playerClassInfo);
        }
        SetParty(_playerClassInfos);
    }

    public void SetParty(List<PlayerClassInfo> playerClassInfos)
    {
        for (int i = 0; i < playerClassInfos.Count; i++)
        {
            _gameManager.Party.AddToParty(playerClassInfos[i]._characterInfoData);
        }
    }

    // public static void SetPlayerStats(List<int> rolledStats)
    // {
    //     _statController.UpdateStats(_characterInfoData, rolledStats);
    // }



}
