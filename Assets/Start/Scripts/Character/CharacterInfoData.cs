using System.Collections.Generic;
using Start.Scripts.Classes;
using Start.Scripts.Inventory;
using Start.Scripts.Levels;
using UnityEngine;
using System;

namespace Start.Scripts.Character
{
    [Serializable]
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "RPG/Character Info", order = 1)]
    public class CharacterInfoData : ScriptableObject 
    {
        public int Id { get; set; }
        public PlayerClass PlayerClass { get; set; }
        public Level CurrentLevel { get; set; }
        public OverlayTile TilePos { get; set; }

        public InventoryHolder Inventory { get; set; }
        public List<InventoryItemData> InventoryItems { get; set; }

        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Mana { get; set; }
        public int MaxMana { get; set; }
        public int Special { get; set; }
        public int MaxSpecial { get; set; }
        public int Experience { get; set; }
        public int MaxExperience { get; set; }
        public int BonusToHit { get; set; }
        public int ArmorClass { get; set; }
        public float Speed { get; set; }
        public int Movement { get; set; }
        public int Initiative { get; set; }

        public InventoryItemData EquippedWeapon { get; set; }
        public InventoryItemData EquippedArmor { get; set; }
        public List<Ability> Abilities { get; set; }

        public Dictionary<string, int> Stats { get; set; }
        public List<int> Modifiers { get; set; }

        public bool HasDisadvantage { get; set; }
        public bool HasAdvantage { get; set; }
        public bool HasMovement { get; set; }
        public bool HasAction { get; set; }
    }

}

