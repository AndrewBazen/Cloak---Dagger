using System.Collections.Generic;
using UnityEngine;

namespace Start.Scripts.Level
{
    public class DungeonManager : MonoBehaviour
    {
        public List<Room> dungeon;

        private void Start()
        {
            dungeon = new List<Room>();
        }
    }
}
