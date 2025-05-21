using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
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

        public void UpdateModifiers(CharacterInfo player)
        {
            var counter = 0;
            foreach (var mod in player.modifiers)
            {
                modifiers[counter].GetComponent<TextMeshPro>().text = mod.ToString();
                counter += 1;
            }
        }

        public void UpdateStatDisplay(CharacterInfo player)
        {
            UpdateModifiers(player);
            manaBar.maxValue = player.maxMana;
            manaBar.value = player.mana;
            
            healthBar.maxValue = player.maxHealth;
            healthBar.value = player.health;
            
            specialBar.maxValue = player.maxSpecial;
            specialBar.value = player.special;
            
            experienceBar.maxValue = player.maxExperience;
            experienceBar.value = player.experience;
        }
    }
}
