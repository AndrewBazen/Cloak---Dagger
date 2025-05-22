using System.Collections.Generic;
using Start.Scripts.Dice;

namespace Start.Scripts
{
    public abstract class Spell
    {
        private SpellData Data { get; }

        private string _dmgDice;
        private DiceController _diceController;


        protected Spell (SpellData source)
        {
            Data = source;
        }
        
    }
}