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
        public int Id { get; set; } = Guid.NewGuid().GetHashCode();
        public PlayerClass PlayerClass { get; set; }
        public Level CurrentLevel { get; set; }
        public int Level { get; set; } = 1;
        public Vector3 Position { get; set; }
        public OverlayTile TilePos { get; set; }

        public InventoryHolder Inventory { get; set; }
        public List<InventoryItemData> InventoryItems { get; set; } = new List<InventoryItemData>();

        public int Health { get; set; } = 10;
        public int MaxHealth { get; set; } = 10;
        public int Mana { get; set; } = 10;
        public int MaxMana { get; set; } = 10;
        public int Special { get; set; } = 0;
        public int MaxSpecial { get; set; } = 0;
        public int Experience { get; set; } = 0;
        public int BonusToHit { get; set; } = 0;
        public int ArmorClass { get; set; } = 0;
        public float Speed { get; set; } = 3;
        public int Movement { get; set; } = 3;
        public int Initiative { get; set; } = 0;

        public InventoryItemData EquippedWeapon { get; set; }
        public InventoryItemData EquippedArmor { get; set; }
        public List<Ability> Abilities { get; set; } = new List<Ability>();

        public Dictionary<string, int> Stats { get; set; } = new Dictionary<string, int>()
        {
            { "Str", 0 },
            { "Dex", 0 },
            { "Con", 0 },
            { "Int", 0 },
            { "Wis", 0 },
            { "Cha", 0 },
        };
        public Dictionary<string, int> Modifiers { get; set; } = new Dictionary<string, int>()
        {
            { "Str", 0 },
            { "Dex", 0 },
            { "Con", 0 },
            { "Int", 0 },
            { "Wis", 0 },
            { "Cha", 0 },
        };

        public bool HasDisadvantage { get; set; } = false;
        public bool HasAdvantage { get; set; } = false;
        public bool HasMovement { get; set; } = false;
        public bool HasAction { get; set; } = false;
    }

}

