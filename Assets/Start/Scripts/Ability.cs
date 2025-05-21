using System;
using UnityEngine;

namespace Start.Scripts
{
    [Serializable]
    [CreateAssetMenu(fileName = "Ability", menuName = "ScriptableObjects/Ability", order = 1)]
    public class Ability : ScriptableObject
    {
        public int areaOfEffect;
        public int attackRange;
        public int averageDmg;
        public int manaCost;
        public string stat;
    }
}