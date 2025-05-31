using System.Collections.Generic;
using UnityEngine;

namespace Start.Scripts.Levels
{
    public class Level : MonoBehaviour, ILevel
    {
        public LevelData LevelData
        {
            get => levelData;
            set => levelData = value;
        }

        private LevelData levelData;

        public List<OverlayTile> PlayerSpawnTiles;
        public List<OverlayTile> EnemySpawnTiles;


        public Level(string Name, string scene, int difficulty)
        {
            levelData = ScriptableObject.CreateInstance<LevelData>();
            levelData.levelName = Name;
            levelData.sceneName = scene;
            levelData.difficulty = difficulty;
            foreach (var tile in levelData.enemySpawnTiles)
            {
                EnemySpawnTiles.Add(tile.GetComponent<OverlayTile>());
            }
            foreach (var tile in levelData.playerSpawnTiles)
            {
                PlayerSpawnTiles.Add(tile.GetComponent<OverlayTile>());
            }
        }
        public void BuildLevel()
        {
            throw new System.NotImplementedException();
        }
    }
}
