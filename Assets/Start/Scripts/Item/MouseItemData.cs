using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Start.Scripts.Item
{
    public class MouseItemData : MonoBehaviour
    {
        public Image ItemSprite;
        public TextMeshProUGUI ItemCount;

        private void Awake()
        {
            ItemSprite.color = Color.clear;
            ItemCount.text = "";
        }
    }
}