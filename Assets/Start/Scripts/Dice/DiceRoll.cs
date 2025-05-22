using System.Collections.Generic;
using System.Linq;
using Start.Scripts.Enemy;
using UnityEngine;
using Random = System.Random;

namespace Start.Scripts.Dice
{
    public class DiceRoll
    {
        private Random _random;
        public int RollToHit(CharacterInfo info)
        {
            var hitBonus = info.bonusToHit;
            var rolls = new List<int>();
            if (!info.hasAdvantage)
            {
                var roll = RollDice("D20", 1);
                return roll.Keys.First();
            }
            if (info.hasDisadvantage)
            {
                rolls = RollDice("D20", 2).Values.First();
                return rolls.Min();
            }
            rolls = RollDice("D20", 2).Values.First();
            return rolls.Max();
            
        }
        
        public int RollToHit(EnemyController info)
        {
            var hitBonus = info.bonusToHit;
            var rolls = new List<int>();
            if (!info.hasAdvantage)
            {
                var roll = RollDice("D20", 1);
                return roll.Keys.First();
            }
            if (info.hasDisadvantage)
            {
                rolls = RollDice("D20", 2).Values.First();
                return rolls.Min();
            }
            rolls = RollDice("D20", 2).Values.First();
            return rolls.Max();
            
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

        public int RollDmg(CharacterInfo other)
        {
            var dmgRoll = RollDice(other.weapon.dmgDice, other.weapon.dmgDiceNum);
            var dmg = dmgRoll.Keys.First();
            return dmg;
        }
        
        public int RollDmg(EnemyController other)
        {
            var dmgRoll = RollDice(other.weapon.dmgDice, other.weapon.dmgDiceNum);
            var dmg = dmgRoll.Keys.First();
            return dmg;
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
                default :
                    Debug.Log("invalid Dice Type");
                    return null;
            }
        }
        
        
    }
}