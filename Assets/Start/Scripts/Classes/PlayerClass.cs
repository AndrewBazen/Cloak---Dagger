using System;
using System.Collections.Generic;
using Start.Scripts.Inventory;
using UnityEngine;

namespace Start.Scripts.Classes
{
    [Serializable]
    [CreateAssetMenu(fileName = "PlayerClass", menuName = "ScriptableObjects/PlayerClass", order = 1)]
    public class PlayerClass : ScriptableObject
    {
        public Sprite playerPic;
        public string className = "";
        public string classDescription = "";
        public List<int> statModifiers;
        public InventoryItemData weapon;
        public InventoryItemData armor;
        public List<Ability> abilities;
    }
}
