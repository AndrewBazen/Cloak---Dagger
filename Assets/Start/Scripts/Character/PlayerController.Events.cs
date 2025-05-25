using System.ComponentModel;
using UnityEngine;
using Start.Scripts.Game;

namespace Start.Scripts.Character
{
    public partial class PlayerController : MonoBehaviour, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void SubscribeToEvents()
        {
            if (GameEvents.current != null)
            {
                GameEvents.current.OnLoadEvent += DestroyMe;
                GameEvents.current.OnTurnStart += (_combatController.StartTurn, GetInRangeTiles);
                GameEvents.current.OnTurnEnd += (_combatController.StopTurn, ResetTiles);
                if (characterData != null)
                {
                    GameEvents.current.OnCharacterStatsChanged += (stats) => characterData.stats = stats;
                    GameEvents.current.OnCharacterInventoryChanged += (inventory) => characterData.inventory = inventory;
                }
                GameEvents.current.OnEnemiesChanged += (enemies) => _enemies = enemies;
                GameEvents.current.BroadcastMessage();
            }
            else
            {
                Debug.LogWarning("GameEvents.current is null. Event subscriptions skipped.");
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
                _gameManager.RemoveFromParty(characterData.id);
            }

            Destroy(gameObject);
        }

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

