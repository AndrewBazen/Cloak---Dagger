using System;
using System.Collections.Generic;
using Start.Scripts.Classes;
using Start.Scripts.Inventory;
using Start.Scripts.UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Start.Scripts
{
    [Serializable]
    public class CharacterInfo : MonoBehaviour
    {
        [SerializeField] public PlayerClass playerClass;
        [SerializeField] public int startExp = 100;
        [SerializeField] public InventoryHolder inventory;
        public List<InventoryItemData> inventoryItems;
        [SerializeField] public int health;
        [SerializeField] public int maxHealth;
        [SerializeField] public int mana;
        [SerializeField] public int maxMana;
        [SerializeField] public int special;
        [SerializeField] public int maxSpecial;
        [SerializeField] public int experience;
        [SerializeField] public int maxExperience;
        [SerializeField] public int bonusToHit;
        [SerializeField] public int armorClass;
        [SerializeField] public InventoryItemData weapon;
        [SerializeField] public InventoryItemData armor;
        [SerializeField] public List<Ability> abilities;
        public StatDisplay statDisplay;
        public string id;
        public List<int> stats;
        public Dictionary<string, int> skills;
        public List<int> modifiers;
        public bool hasDisadvantage;
        public bool hasAdvantage;
        public bool hasMovement;
        public bool hasAttack;
        public OverlayTile standingOnTile;
        public int initiative;

        private void Awake()
        {
            // create a specific player ID
            id = DateTime.Now.ToLongDateString() + DateTime.Now.ToLongDateString() +
                        Random.Range(0, int.MaxValue);
            // initiate the player's stats
            skills = new Dictionary<string, int>()
            {
                {"Str", -100},
                {"Dex", -100},
                {"Con", -100},
                {"Int", -100},
                {"Wis", -100},
                {"Cha", -100}
            };
            
            // adjust the modifiers based on the chosen class
            for (var i = 0; i < modifiers.Count; i++)
            {
                modifiers[i] += playerClass.statModifiers[i];
            }
        }
    }
}