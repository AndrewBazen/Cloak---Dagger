using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Start.Scripts.Character;

namespace Start.Scripts.UI
{
    public class StatController
    {

        // updates the players stats based off input and then recalculated modifiers
        public void UpdateStats(CharacterInfoData player, Dictionary<string, int> statsToUpdate)
        {
            var playerStats = player.skills;
            foreach (var item in statsToUpdate)
            {
                if (playerStats.ContainsKey(item.Key))
                {
                    var currentStat = playerStats[item.Key];
                    playerStats[item.Key] = currentStat + item.Value;
                }
            }
            UpdateModifiers(player);
        }

        public void AdjustClassModifiers(CharacterInfoData player)
        {
            for (var i = 0; i < player.modifiers.Count; i++)
            {
                player.modifiers[i] += player.playerClass.statModifiers[i];
            }
        }

        private void UpdateModifiers(CharacterInfoData player)
        {
            List<int> tempMods = new List<int>(6);
            foreach (var stat in player.skills.Values)
            {
                switch (stat)
                {
                    case 6 or 7:
                        tempMods.Add(-2);
                        break;
                    case 8 or 9:
                        tempMods.Add(-1);
                        break;
                    case 10 or 11:
                        tempMods.Add(0);
                        break;
                    case 12 or 13:
                        tempMods.Add(1);
                        break;
                    case 14 or 15:
                        tempMods.Add(2);
                        break;
                    case 16 or 17:
                        tempMods.Add(3);
                        break;
                    case 18 or 19:
                        tempMods.Add(4);
                        break;
                    case 20 or 21:
                        tempMods.Add(5);
                        break;
                    case 22 or 23:
                        tempMods.Add(6);
                        break;
                    case 24 or 25:
                        tempMods.Add(7);
                        break;
                    case 26 or 27:
                        tempMods.Add(8);
                        break;
                    case 28 or 29:
                        tempMods.Add(9);
                        break;
                    case 30:
                        tempMods.Add(10);
                        break;
                    case < 6:
                        tempMods.Add(-3);
                        break;
                    default:
                        tempMods.Add(0);
                        break;
                }
            }

            player.modifiers = tempMods;
        }
    }
}
