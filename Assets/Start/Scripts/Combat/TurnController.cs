using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



namespace Start.Scripts
{
    public class TurnController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup turnPanel;
        [SerializeField] private List<Button> buttons;

        public CanvasGroup TurnPanel => turnPanel;
        

        private void Awake()
        {
            turnPanel = GameObject.FindGameObjectWithTag("TurnPanel").GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            GetButtons();
            buttons = new List<Button>();
        }

        private void GetButtons()
        {
            for (int i = 0; i < turnPanel.transform.childCount; i++)
            {
                if (turnPanel.transform.GetChild(i).CompareTag("TurnPanelButton"))
                {
                    buttons.Add(turnPanel.transform.GetChild(i).GetComponent<Button>());
                }
            }
        }
    }
}