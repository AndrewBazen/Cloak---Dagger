// CharacterLoadData.cs
using Start.Scripts.Classes;
using UnityEngine;

namespace Start.Scripts.Character
{
    [System.Serializable]
    public class CharacterLoadData
    {
        public string characterName;
        public PlayerClass playerClass;
        public Vector3 position; // optional if using tile-based spawning
        // Add other saved properties as needed
    }
}

