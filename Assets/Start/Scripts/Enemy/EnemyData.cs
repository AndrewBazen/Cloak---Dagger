using System;
using System.Collections.Generic;
using Start.Scripts.Inventory;
using UnityEngine;


namespace Start.Scripts.Enemy
{
    [Serializable]
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Enemy", order = 1)]
    public class EnemyData : ScriptableObject
    {
        [SerializeField] public InventoryItemData weapon;
        [SerializeField] public InventoryItemData armor;
        
        public List<Ability> abilities;
        public List<string> statType;
        public List<int> stats;
        public int health;
        public int maxHealth;
        public int mana;
        public int maxMana;
        public float speed;
        public int movement;
        public string enemyType;
        public bool hasDisadvantage;
        public bool hasAdvantage;
        public bool hasMovement;
        public bool hasAttack;
    }
}
