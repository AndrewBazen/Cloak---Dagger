using System;
using System.Collections.Generic;
using UnityEngine;

namespace Start.Scripts.Game
{
    public class AddToGameManager : MonoBehaviour
    {
        public void AddPartyMember()
        {
            GameManager.Instance.party.Clear();
            var players = new List<Transform>();
            for (var i = 0; i < gameObject.transform.childCount; i++)
            {
                players.Add(gameObject.transform.GetChild(i));   
            }
            foreach (var player in players)
            {
                var info = player.GetComponentInChildren<CharacterInfo>();
                if (info.playerClass != null)
                {
                    GameManager.Instance.party.Add(info);
                }
            }
            
        }
    }
}