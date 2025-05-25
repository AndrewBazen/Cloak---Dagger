using System.Collections.Generic;
using Start.Scripts.Classes;
using Start.Scripts.Character;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Start.Scripts.UI
{
    public class ClassDropDown : MonoBehaviour
    {
        public TMP_Dropdown dropdown;
        public Image playerPic;
        public CharacterInfoData player;
        public List<PlayerClass> classes;


        private delegate void ClassDelegate();
        ClassDelegate _classDelegate;

        private void Start()
        {
            dropdown = gameObject.GetComponent<TMP_Dropdown>();
            _classDelegate += UpdatePlayerPic;
            _classDelegate += UpdatePlayerInfo;
        }

        public void RunItems()
        {
            _classDelegate();
        }

        private void UpdatePlayerPic()
        {
            foreach (var option in dropdown.options)
            {
                if (dropdown.value != dropdown.options.IndexOf(option))
                {
                    continue;
                }

                dropdown.itemImage.sprite = dropdown.options[dropdown.value].image;
                playerPic.sprite = dropdown.itemImage.sprite;
                return;
            }
        }

        private void UpdatePlayerInfo()
        {
            if (dropdown.value == 0)
            {
                player.playerClass = null;
                return;
            }
            foreach (var option in dropdown.options)
            {
                if (dropdown.value != dropdown.options.IndexOf(option))
                {
                    continue;
                }

                foreach (var c in classes)
                {
                    Debug.Log(dropdown.itemText.text);
                    if (c.className != dropdown.options[dropdown.value].text)
                    {
                        continue;
                    }

                    player.playerClass = c;
                    return;
                }
            }
        }
    }
}
