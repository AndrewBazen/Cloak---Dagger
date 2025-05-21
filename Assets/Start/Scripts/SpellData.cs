using UnityEngine;

namespace Start.Scripts
{
    [CreateAssetMenu(menuName = "Spell Data")]
    public class SpellData : ScriptableObject
    {
        public string id;
        public string spellName;
        public Sprite icon;
        public GameObject itemPrefab;
        public string spellType;
        public int spellRange;
    }
}