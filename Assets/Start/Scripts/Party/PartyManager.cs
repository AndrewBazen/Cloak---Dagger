using System;
using System.Collections.Generic;
using UnityEngine;

namespace Start.Scripts.Party
{
    [Serializable]
    public class PartyManager : MonoBehaviour
    {
        private static PartyManager _instance;

        public static PartyManager Instance => _instance;
        
        public List<CharacterInfo> party;
        [SerializeField] public GameObject statDisplayPrefab;
        [SerializeField] public GameObject statDisplayContainer;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            } else
            {
                _instance = this;
            }
        }
    }
}