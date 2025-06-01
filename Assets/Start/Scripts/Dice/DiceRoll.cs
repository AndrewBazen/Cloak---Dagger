using System.Collections.Generic;
using System.Linq;
using Start.Scripts.Enemy;
using UnityEngine;
using Random = System.Random;
using Start.Scripts.Character;

namespace Start.Scripts.Dice
{
    public class DiceRoll
    {
        private Random _random;
        public int RollToHit(PlayerController player)
        {
            var info = player.characterData;
            if (!info.HasAdvantage)
            {
                return RollDice("D20", 1).Keys.First();
            }
            if (info.HasDisadvantage)
            {
                return RollDice("D20", 2).Values.First().Min();
            }
            return RollDice("D20", 2).Values.First().Max();

        }
        public int RollToHit(EnemyController enemy)
        {
            var info = enemy.EnemyData;
            if (!info.HasAdvantage)
            {
                return RollDice("D20", 1).Keys.First();
            }
            if (info.HasDisadvantage)
            {
                return RollDice("D20", 2).Values.First().Min();
            }
            return RollDice("D20", 2).Values.First().Max();
        }
        public int RollStat()
        {
            var statRoll = 0;
            var roll = new DiceRoll();

            while (statRoll < 7)
            {
                var rolls = roll.RollDice("D6", 4).Values.First();
                rolls.Remove(rolls.Min());
                statRoll = 0;
                foreach (var r in rolls)
                {
                    statRoll += r;
                }
            }
            return statRoll;
        }

        public int RollDmg(PlayerController other)
        {
            return RollDice(other.characterData.EquippedWeapon.dmgDice, other.characterData.EquippedWeapon.dmgDiceNum).Keys.First();
        }
        public int RollDmg(EnemyController other)
        {
            return RollDice(other.EnemyData.EquippedWeapon.dmgDice, other.EnemyData.EquippedWeapon.dmgDiceNum).Keys.First();
        }

        public Dictionary<int, List<int>> RollDice(string dice, int numberOfDice)
        {
            _random = new Random();
            var result = new Dictionary<int, List<int>>();
            var rolls = new List<int>();
            int rollTotal = 0;
            switch (dice)
            {
                case "D20":
                    for (int i = 0; i < numberOfDice; i++)
                    {
                        var roll = _random.Next(1, 21);
                        rolls.Add(roll);
                        rollTotal += roll;
                    }
                    result[rollTotal] = rolls;
                    return result;
                case "D12":
                    for (int i = 0; i < numberOfDice; i++)
                    {
                        var roll = _random.Next(1, 13);
                        rolls.Add(roll);
                        rollTotal += roll;
                    }
                    result[rollTotal] = rolls;
                    return result;
                case "D10":
                    for (int i = 0; i < numberOfDice; i++)
                    {
                        var roll = _random.Next(1, 11);
                        rolls.Add(roll);
                        rollTotal += roll;
                    }
                    result[rollTotal] = rolls;
                    return result;
                case "D8":
                    for (int i = 0; i < numberOfDice; i++)
                    {
                        var roll = _random.Next(1, 9);
                        rolls.Add(roll);
                        rollTotal += roll;
                    }
                    result[rollTotal] = rolls;
                    return result;
                case "D6":
                    for (int i = 0; i < numberOfDice; i++)
                    {
                        var roll = _random.Next(1, 7);
                        rolls.Add(roll);
                        rollTotal += roll;
                    }
                    result[rollTotal] = rolls;
                    return result;
                case "D4":
                    for (int i = 0; i < numberOfDice; i++)
                    {
                        var roll = _random.Next(1, 5);
                        rolls.Add(roll);
                        rollTotal += roll;
                    }
                    result[rollTotal] = rolls;
                    return result;
                default:
                    Debug.Log("invalid Dice Type");
                    return null;
            }
        }

    }
}
