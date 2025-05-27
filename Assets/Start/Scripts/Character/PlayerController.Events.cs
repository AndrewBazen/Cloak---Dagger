using System.ComponentModel;
using UnityEngine;
using Start.Scripts.Game;

namespace Start.Scripts.Character
{
    public partial class PlayerController
    {
        private void SubscribeToEvents()
        {
            if (GameEvents.current != null)
            {
                GameEvents current = GameEvents.current;
                current.OnLoadEvent += DestroyMe;
                current.OnEnemiesChanged += (enemies) => _enemies = enemies;
            }
            else
            {
                Debug.LogWarning("GameEvents.current is null. Event subscriptions skipped.");
            }
        }
        public void StartTurn()
        {
            if (_combatController != null)
            {
                _combatController.StartTurn();
            }
            else
            {
                Debug.LogWarning("CombatController is not initialized. Cannot start turn.");
            }
        }

        public void EndTurn()
        {
            if (_combatController != null)
            {
                _combatController.StopTurn();
            }
            else
            {
                Debug.LogWarning("CombatController is not initialized. Cannot end turn.");
            }
        }
        private void DestroyMe()
        {
            if (GameEvents.current != null)
            {
                GameEvents.current.OnLoadEvent -= DestroyMe;
            }

            if (_gameManager != null && characterData != null)
            {
                _gameManager.Party.RemoveFromParty(this.gameObject);
            }

            Destroy(gameObject);
        }

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

