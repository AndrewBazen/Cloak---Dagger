using System;
using UnityEngine;

namespace Start.Scripts.Inventory
{
    [Serializable]
    [CreateAssetMenu(menuName = "Inventory System/Inventory Item")]
    public class InventoryItemData : ScriptableObject
    {
        public string id;
        public string itemName;
        public string dmgDice;
        public int dmgDiceNum;
        public string twoHandedDmgDice;
        public string weaponType;
        public int weaponRange;
        public float averageDmg;
        public string weaponStat;
        public int armorClassBonus;
        [TextArea(4, 4)] public string description;
        public Sprite icon;
        public int maxStackSize;

        // ✨ Optional AI behavior hints for enemy combat logic
        public WeaponAIHints aiHints;
    }

    [Serializable]
    public class WeaponAIHints
    {
        public int preferredRange = 1;
        public bool prefersCover = false;
        public bool avoidsMelee = false;
        public bool flankPriority = false;
    }
}
