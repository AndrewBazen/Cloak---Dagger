
using System.Collections.Generic;
using Start.Scripts.Dice;
using TMPro;
using UnityEngine;
using Start.Scripts.Character;


namespace Start.Scripts.UI
{
    public class StatRoll : MonoBehaviour
    {
        private DiceRoll _diceRoll;
        private TMP_Text _rollText;
        public GameObject roll;
        public StatDropDownController statDropDownController;
        public List<TMP_Dropdown> statDropDowns;
        private CharacterInfoData _player;

        private void Start()
        {
            _player = gameObject.GetComponentInParent<PlayerController>().characterData;
            _rollText = roll.GetComponent<TMP_Text>();
            _diceRoll = new DiceRoll();
        }

        public void RollStatButton()
        {
            if (_rollText.text == "0")
            {
                var emptyStats = 0;
                var stat = _diceRoll.RollStat();
                _rollText.text = stat.ToString();
                foreach (var option in statDropDowns[0].options)
                {
                    if (option.text == "--")
                    {
                        emptyStats += 1;
                    }
                }

                var currIndex = statDropDowns[0].options.Count - emptyStats + 1;


                foreach (var dropdown in statDropDowns)
                {
                    dropdown.options[currIndex].text = stat.ToString();
                }
                statDropDownController.currOptionPool.Add(new TMP_Dropdown.OptionData(stat.ToString()), currIndex);
                _player.stats.Add(stat);
            }
        }
    }
}
