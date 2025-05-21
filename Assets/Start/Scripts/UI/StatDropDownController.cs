using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Start.Scripts.UI
{
    public class StatDropDownController: MonoBehaviour
    {
        public Dictionary<TMP_Dropdown.OptionData, int> removedOptions;
        public Dictionary<TMP_Dropdown.OptionData, int> currOptionPool;
        public List<int> disabledIndexes;
        private void Awake()
        {
            removedOptions = new Dictionary<TMP_Dropdown.OptionData, int>();
            currOptionPool = new Dictionary<TMP_Dropdown.OptionData, int>();
            disabledIndexes = new List<int>();
        }
    }
}