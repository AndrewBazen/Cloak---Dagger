using System;
using System.Collections.Generic;
using TMPro;
using Start.Scripts.Character;
using UnityEngine;
using UnityEngine.UI;

namespace Start.Scripts.UI
{
    [Serializable]
    public class StatDisplay : MonoBehaviour
    {
        public GameObject PlayerClass;
        public GameObject ArmorClass;
        public GameObject Initiative;
        public GameObject Level;
        public Slider manaBar;
        public Slider healthBar;
        public Slider specialBar;
        public Slider experienceBar;
        public List<GameObject> modifiers;

        private void Awake()
        {
            modifiers = new List<GameObject>(6);
        }

        public void UpdateModifiers(CharacterInfoData player)
        {
            for (var i = 0; i < player.PlayerClass.statModifiers.Count; i++)
            {
                modifiers[i].GetComponent<TextMeshPro>().text = player.PlayerClass.statModifiers[i].ToString();
            }
        }

        public void UpdateStats(CharacterInfoData player)
        {
            UpdateModifiers(player);
            manaBar.maxValue = player.MaxMana;
            manaBar.value = player.Mana;
            healthBar.maxValue = player.MaxHealth;
            healthBar.value = player.Health;
            specialBar.maxValue = player.MaxSpecial;
            specialBar.value = player.Special;
            experienceBar.maxValue = player.MaxExperience;
            experienceBar.value = player.Experience;
        }
    }
}
