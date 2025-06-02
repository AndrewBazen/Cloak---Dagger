using System.Collections.Generic;
using System.Linq;
using Start.Scripts.Character;

namespace Start.Scripts.UI
{
    public class StatController
    {
        private List<string> _stats = new() {
            "Strength",
            "Dexterity",
            "Constitution",
            "Intelligence",
            "Wisdom",
            "Charisma",
        };

        // updates the players _enemyData.stats based off input and then recalculate modifiers
        public void UpdateStats(CharacterInfoData player, List<int> rolledStats)
        {
            var playerStats = player.Stats;
            for (var i = 0; i < rolledStats.Count; i++)
            {
                playerStats[_stats[i]] = rolledStats[i];
            }
            UpdateModifiers(player);
            AddClassModifiers(player);
        }

        public void AddClassModifiers(CharacterInfoData player)
        {
            for (var i = 0; i < player.PlayerClass.statModifiers.Count; i++)
            {
                player.Modifiers[_stats[i]] += player.PlayerClass.statModifiers[_stats[i]];
            }
        }

        private void UpdateModifiers(CharacterInfoData player)
        {
            for (var i = 0; i < player.Stats.Count; i++)
            {
                switch (player.Stats[_stats[i]])
                {
                    case 6 or 7:
                        player.Modifiers[_stats[i]] = -2;
                        break;
                    case 8 or 9:
                        player.Modifiers[_stats[i]] = -1;
                        break;
                    case 10 or 11:
                        player.Modifiers[_stats[i]] = 0;
                        break;
                    case 12 or 13:
                        player.Modifiers[_stats[i]] = 1;
                        break;
                    case 14 or 15:
                        player.Modifiers[_stats[i]] = 2;
                        break;
                    case 16 or 17:
                        player.Modifiers[_stats[i]] = 3;
                        break;
                    case 18 or 19:
                        player.Modifiers[_stats[i]] = 4;
                        break;
                    case 20 or 21:
                        player.Modifiers[_stats[i]] = 5;
                        break;
                    case 22 or 23:
                        player.Modifiers[_stats[i]] = 6;
                        break;
                    case 24 or 25:
                        player.Modifiers[_stats[i]] = 7;
                        break;
                    case 26 or 27:
                        player.Modifiers[_stats[i]] = 8;
                        break;
                    case 28 or 29:
                        player.Modifiers[_stats[i]] = 9;
                        break;
                    case 30:
                        player.Modifiers[_stats[i]] = 10;
                        break;
                    case < 6:
                        player.Modifiers[_stats[i]] = -3;
                        break;
                    default:
                        player.Modifiers[_stats[i]] = 0;
                        break;
                }
            }
        }
    }
}
