using System.Collections.Generic;
using Start.Scripts.Enemy;
using UnityEngine;

namespace Start.Scripts.Level
{
    public class Level : MonoBehaviour, ILevel
    {
        public List<EnemyData> enemies;
        private List<OverlayTile> _enemySpawnTiles;

        public void BuildLevel()
        {
            throw new System.NotImplementedException();
        }
    }
}
