using UnityEngine;
using System.Collections.Generic;

namespace Start.Scripts.Levels
{
    /// <summary>
    /// Represents the data for a level in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
    public class LevelData : ScriptableObject
    {
        // TODO: hook this up to the level class and game manager for saving and loading
        public string levelName; // Name of the level
        public string sceneName; // Name of the scene to load for this level
        public int difficulty; // Difficulty rating of the level
        public string description; // Description of the level
        public Sprite thumbnail; // Thumbnail image for the level
        public List<GameObject> playerSpawnTiles; // List of tiles where players can spawn
        public List<GameObject> enemySpawnTiles; // List of tiles where enemies can spawn
    }
}
