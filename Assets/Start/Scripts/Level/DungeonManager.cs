using System;
using System.Collections.Generic;
using Start.Scripts.Level;
using UnityEngine;

namespace Start.Scripts
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