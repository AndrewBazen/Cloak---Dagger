using System;
using System.Collections.Generic;
using Start.Scripts.Enemy;
using CharacterInfo = Start.Scripts.Character.CharacterInfo;

namespace Start.Scripts.Party
{
    [Serializable]
    public class GameData
    {
        public List<CharacterInfo> party;
        public List<EnemyData> enemies;

        public GameData(List<CharacterInfo> party, List<EnemyData> enemies)
        {
            this.party = party;
            this.enemies = enemies;
        }
    }
}
