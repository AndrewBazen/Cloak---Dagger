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
        [SerializeField] public InventoryItemData EquippedWeapon;
        [SerializeField] public InventoryItemData EquippedArmor;
        public List<Ability> Abilities;
        public List<string> StatType;
        public List<int> Stats;
        public Dictionary<string, int> Modifiers;
        public OverlayTile TilePosition;
        public Vector3 Position;
        public string AttackType;
        public int Health;
        public int MaxHealth;
        public int Mana;
        public int MaxMana;
        public float Speed;
        public int Movement;
        public int BonusToHit;
        public bool HasDisadvantage;
        public bool HasAdvantage;
        public bool HasMovement;
        public bool HasAttack;
        public bool HasBonusAction;
        public bool HasAction;
        public bool HasReaction;
        public bool HasTurn;
        public int Initiative;
        public int BaseArmorClass;
    }
}
