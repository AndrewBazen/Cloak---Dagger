using System;
using System.Collections.Generic;
using Start.Scripts.Enemy;
using Start.Scripts.Character;

namespace Start.Scripts.Party
{
    [Serializable]
    public class GameData
    {
        public List<CharacterInfoData> party;
        public List<EnemyData> enemies;

        public GameData(List<CharacterInfoData> party, List<EnemyData> enemies)
        {
            this.party = party;
            this.enemies = enemies;
        }
    }
}
