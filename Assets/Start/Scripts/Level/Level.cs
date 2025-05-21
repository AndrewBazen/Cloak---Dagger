using System.Collections.Generic;
using JetBrains.Annotations;
using Start.Scripts.Enemy;
using UnityEngine;
using UnityEngine.Serialization;

namespace Start.Scripts.Level
{
    public abstract class Level : MonoBehaviour, ILevel
    {
        public List<EnemyData> enemies;
        private List<OverlayTile> _enemySpawnTiles;

        public void BuildLevel()
        {
            throw new System.NotImplementedException();
        }
    }
}