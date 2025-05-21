using System;
using Start.Scripts.Game;
using Start.Scripts.Party;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Start.Scripts.UI
{
    public class UIController : MonoBehaviour
    {

        [SerializeField] private GameObject escapeMenu;

        private void Start()
        {
            GameEvents.current.OnEscPressed += OpenEscapeMenu;
            GameEvents.current.OnEscPressed += Resume;
            GameEvents.current.OnEscPressed += Pause;
        }

        private void Awake()
        {
            DisplayPartyStats();
        }

        private void OpenEscapeMenu()
        {
            if (SceneManager.GetActiveScene().name == "Main")
            {
                escapeMenu.SetActive(!escapeMenu.activeSelf);
            }
        }

        private void Resume()
        {
            if (!escapeMenu.activeSelf)
            {
                Time.timeScale = 1f;
            }
        }

        private void Pause()
        {
            if (escapeMenu.activeSelf)
            {
                Time.timeScale = 0f;
            }
        }
        
        public void DisplayPartyStats()
        {
            foreach (var player in PartyManager.Instance.party)
            {
                player.statDisplay = Instantiate(PartyManager.Instance.statDisplayPrefab, PartyManager.Instance.statDisplayContainer.transform).GetComponent<StatDisplay>();
                player.statDisplay.UpdateStatDisplay(player);
                player.statDisplay.UpdateModifiers(player);
            }
        }
    }
}
