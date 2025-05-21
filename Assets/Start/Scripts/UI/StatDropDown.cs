using TMPro;
using UnityEngine;

namespace Start.Scripts.UI
{
    public class StatDropDown : MonoBehaviour
    {
        public TMP_Dropdown dropdown;
        public StatDropDownController _statDropDownController;
        public TMP_Dropdown.OptionData PreviousOptionData;

        

        private delegate void ClassDelegate();
        ClassDelegate _classDelegate;

        private void Start()
        {
            dropdown = gameObject.GetComponent<TMP_Dropdown>();
            PreviousOptionData = new TMP_Dropdown.OptionData();
            _classDelegate += UpdateDropDownOptions;
        }

        public void RunItems()
        {
            _classDelegate();
        }

        private void UpdateDropDownOptions()
        {
            var changedValue = dropdown.options[dropdown.value];
            var changedValueIndex = dropdown.options.IndexOf(changedValue);
            var previousOptionDataIndex = dropdown.options.IndexOf(PreviousOptionData);
            
            // if value is changed from "--" to a number
            if (changedValue.text != "--" && PreviousOptionData.text == "--") 
            {
                var dropDownDisable = gameObject.GetComponent<DropDownDisable>();
                dropDownDisable.EnableOption(changedValueIndex, false);
                _statDropDownController.removedOptions[changedValue] = changedValueIndex;
            }else if (changedValue.text == "--")  // if value is changed to "--"
            {
                var dropDownDisable = gameObject.GetComponent<DropDownDisable>();
                dropDownDisable.EnableOption(previousOptionDataIndex,true);
                _statDropDownController.removedOptions.Remove(PreviousOptionData);
            }else if (changedValue.text != "--" && PreviousOptionData.text != "--")  // if value is changed from a number to a number (not the same number)
            {
                var dropDownDisable = gameObject.GetComponent<DropDownDisable>();
                dropDownDisable.EnableOption(changedValueIndex, false);
                if (previousOptionDataIndex >= 0)
                {
                    Debug.Log(previousOptionDataIndex);
                    dropDownDisable.EnableOption(previousOptionDataIndex, true);
                    _statDropDownController.removedOptions.Remove(PreviousOptionData);
                }
                _statDropDownController.removedOptions[changedValue] = changedValueIndex;
            }else if (changedValue.text != "--" && PreviousOptionData.text == changedValue.text) // if value is changed from a number to the same number
            {
                var dropDownDisable = gameObject.GetComponent<DropDownDisable>();
                dropDownDisable.EnableOption(changedValueIndex, false);
                dropDownDisable.EnableOption(previousOptionDataIndex, true);
                _statDropDownController.removedOptions.Remove(PreviousOptionData);
                _statDropDownController.removedOptions[changedValue] = changedValueIndex;
            }
            PreviousOptionData = changedValue;
        }
    }
}
