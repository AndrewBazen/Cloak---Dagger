using UnityEngine;
using UnityEngine.UI;
using Start.Scripts.Classes;
using Start.Scripts.Character;
using Start.Scripts.Game;
using System.Collections.Generic;
using Start.Scripts.UI;

public class PartySelectionInfo : MonoBehaviour
{
    public List<CharacterInfoData> _characterInfoData;
    public List<PlayerClass> chosenClasses;
    public GameManager _gameManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _gameManager = GameManager.Instance;
        _characterInfoData = new List<CharacterInfoData>();
        chosenClasses = new List<PlayerClass>();
    }

    public void SetupPlayer()
    {
        SetPlayerClass(chosenClasses);
    }

    public void SetPlayerClass(List<PlayerClass> chosenClasses)
    {
        for (int i = 0; i < chosenClasses.Count; i++)
        {
            var playerClass = chosenClasses[i];
            _characterInfoData[i].PlayerClass = playerClass;
            _characterInfoData[i].EquippedWeapon = playerClass.weapon;
            _characterInfoData[i].EquippedArmor = playerClass.armor;
            _characterInfoData[i].ArmorClass = playerClass.baseArmorClass + _characterInfoData[i].EquippedArmor.armorClassBonus;
            _characterInfoData[i].BonusToHit = _characterInfoData[i].Modifiers[_characterInfoData[i].EquippedWeapon.weaponStat];
            _characterInfoData[i].Abilities = playerClass.abilities;
            _characterInfoData[i].Health = playerClass.baseHealth;
            _characterInfoData[i].Mana = playerClass.baseMana;
            _characterInfoData[i].MaxHealth = playerClass.baseHealth;
            _characterInfoData[i].MaxMana = playerClass.baseMana;
        }
    }

    // public static void SetPlayerStats(List<int> rolledStats)
    // {
    //     _statController.UpdateStats(_characterInfoData, rolledStats);
    // }



}
