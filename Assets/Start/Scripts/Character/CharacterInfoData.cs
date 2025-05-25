using System;
using System.Collections.Generic;
using Start.Scripts.Classes;
using Start.Scripts.Inventory;
using Start.Scripts.UI;
using UnityEngine;

namespace Start.Scripts.Character
{
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "RPG/Character Info", order = 1)]
    public class CharacterInfoData : ScriptableObject
    {
        public string id;
        public PlayerClass playerClass;
        public int startExp = 100;

        public InventoryHolder inventory;
        public List<InventoryItemData> inventoryItems;

        public int health = 10;
        public int maxHealth = 10;
        public int mana = 10;
        public int maxMana = 10;
        public int special;
        public int maxSpecial;
        public int experience;
        public int maxExperience;
        public int bonusToHit;
        public int armorClass = 10;

        public InventoryItemData weapon;
        public InventoryItemData armor;
        public List<Ability> abilities;

        public List<int> stats;
        public Dictionary<string, int> skills;
        public List<int> modifiers;

        public bool hasDisadvantage;
        public bool hasAdvantage;
        public bool hasMovement;
        public bool hasAttack;
    }
}

