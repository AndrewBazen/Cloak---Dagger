using System;
using System.Collections.Generic;
using Start.Scripts.Classes;
using Start.Scripts.Inventory;
using Start.Scripts.UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Start.Scripts.Character
{
    [Serializable]
    public class CharacterInfo : MonoBehaviour
    {
        [SerializeField] public PlayerClass playerClass;
        [SerializeField] public int startExp = 100;
        [SerializeField] public InventoryHolder inventory;
        public List<InventoryItemData> inventoryItems;
        [SerializeField] public int health = 10;
        [SerializeField] public int maxHealth = 10;
        [SerializeField] public int mana = 10;
        [SerializeField] public int maxMana = 10;
        [SerializeField] public int special;
        [SerializeField] public int maxSpecial;
        [SerializeField] public int experience;
        [SerializeField] public int maxExperience;
        [SerializeField] public int bonusToHit;
        [SerializeField] public int armorClass = 10;
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
            InitializeCharacter();
        }

        private void InitializeCharacter()
        {
            // Create a specific player ID
            id = DateTime.Now.ToLongDateString() + DateTime.Now.ToLongTimeString() +
                        Random.Range(0, int.MaxValue);

            // Initialize collections
            if (inventoryItems == null)
                inventoryItems = new List<InventoryItemData>();

            if (abilities == null)
                abilities = new List<Ability>();

            if (stats == null)
                stats = new List<int>();

            if (modifiers == null)
                modifiers = new List<int>();

            // Initialize skills
            skills = new Dictionary<string, int>
            {
                {"Str", 10},
                {"Dex", 10},
                {"Con", 10},
                {"Int", 10},
                {"Wis", 10},
                {"Cha", 10}
            };

            // Apply class modifiers if class is available
            if (playerClass != null && modifiers.Count > 0 && playerClass.statModifiers.Count > 0)
            {
                for (var i = 0; i < Mathf.Min(modifiers.Count, playerClass.statModifiers.Count); i++)
                {
                    modifiers[i] += playerClass.statModifiers[i];
                }
            }
        }
    }
}
