using UnityEngine;
using static System.Random;

namespace Start.Scripts
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Enemy", order = 1)]
    public class EnemyData : ScriptableObject
    {
        public int hp;
        public int damage;
        public float speed;
        public int initiative;
        public int movement;
    }
}
