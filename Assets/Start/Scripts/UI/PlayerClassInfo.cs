using UnityEngine;
using UnityEngine.UI;
using Start.Scripts.Classes;
using Start.Scripts.Character;
using Start.Scripts.Game;
using System.Collections.Generic;
using Start.Scripts.UI;
using Unity.VisualScripting;

public class PlayerClassInfo : MonoBehaviour
{
    public CharacterInfoData _characterInfoData;
    public PlayerClass chosenClass;
    public GameManager _gameManager;

    private ClassDropDown _classDropDown;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _gameManager = GameManager.Instance;
        _characterInfoData = new CharacterInfoData();
        _classDropDown = gameObject.GetComponentInParent<ClassDropDown>();
    }

    public void SetupPlayer()
    {
        int classIndex = _classDropDown.dropdown.value;
        SetPlayerClass(_classDropDown.classes[classIndex]);
    }

    public void SetPlayerClass(PlayerClass chosenClass)
    {
        _characterInfoData.PlayerClass = chosenClass;
        _characterInfoData.EquippedWeapon = chosenClass.weapon;
        _characterInfoData.EquippedArmor = chosenClass.armor;
        _characterInfoData.ArmorClass = chosenClass.baseArmorClass + _characterInfoData.EquippedArmor.armorClassBonus;
        _characterInfoData.BonusToHit = _characterInfoData.Modifiers[_characterInfoData.EquippedWeapon.weaponStat];
        _characterInfoData.Abilities = chosenClass.abilities;
        _characterInfoData.Health = chosenClass.baseHealth;
        _characterInfoData.Mana = chosenClass.baseMana;
        _characterInfoData.MaxHealth = chosenClass.baseHealth;
        _characterInfoData.MaxMana = chosenClass.baseMana;
    }

    // public static void SetPlayerStats(List<int> rolledStats)
    // {
    //     _statController.UpdateStats(_characterInfoData, rolledStats);
    // }



}

