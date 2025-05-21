using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Start.Scripts.UI
{
    [RequireComponent(typeof(TMP_Dropdown))]
    [DisallowMultipleComponent]
    public class DropDownDisable : MonoBehaviour, IPointerClickHandler
    {
        [Tooltip("Indexes that should be ignored. Indexes are 0 based.")]
    public StatDropDownController StatDropDownController;
    private TMP_Dropdown _dropdown;

    private void Awake()
    {
        _dropdown = GetComponent<TMP_Dropdown>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var dropDownList = GetComponentInChildren<Canvas>();
        if (!dropDownList) return;

        // If the dropdown was opened find the options toggles
        var toggles = dropDownList.GetComponentsInChildren<Toggle>(true);

        // the first item will always be a template item from the dropdown we have to ignore
        // so we start at one and all options indexes have to be 1 based
        for (var i = 1; i < toggles.Length; i++)
        {
            // disable buttons if their 0-based index is in indexesToDisable
            // the first item will always be a template item from the dropdown
            // so in order to still have 0 based indexes for the options here we use i-1
            toggles[i].interactable = !StatDropDownController.disabledIndexes.Contains(i - 1);
        }
    }

    // Anytime change a value by index
    public void EnableOption(int index, bool enable)
    {
        if (enable)
        {
            // remove index from disabled list
            if (StatDropDownController.disabledIndexes.Contains(index)) StatDropDownController.disabledIndexes.Remove(index);
        }
        else
        {
            // add index to disabled list
            if (!StatDropDownController.disabledIndexes.Contains(index)) StatDropDownController.disabledIndexes.Add(index);
        }

        var dropDownList = GetComponentInChildren<Canvas>();

        // If this returns null than the Dropdown was closed
        if (!dropDownList) return;

        // If the dropdown was opened find the options toggles
        var toggles = dropDownList.GetComponentsInChildren<Toggle>(true);
        toggles[index].interactable = enable;
    }

    // Anytime change a value by string label
    public void EnableOption(string label, bool enable)
    {
        var index = _dropdown.options.FindIndex(o => string.Equals(o.text, label));

        // We need a 1-based index
        EnableOption(index + 1, enable);
    }
    }
}