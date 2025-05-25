using System.ComponentModel;
using UnityEngine;
using Start.Scripts.Game;

namespace Start.Scripts.Character
{
    public partial class PlayerController : MonoBehaviour, INotifyPropertyChanged
    {
        private void SubscribeToEvents()
        {
            if (GameEvents.current != null)
            {
                GameEvents current = GameEvents.current;
                current.OnLoadEvent += DestroyMe;
                current.OnTurnStart += (_combatController.StartTurn, GetInRangeTiles);
                current.OnTurnEnd += (_combatController.StopTurn, ResetTiles);
                if (characterData != null)
                {
                    current.OnCharacterStatsChanged += (stats) => characterData.stats = stats;
                    current.OnCharacterInventoryChanged += (inventory) => characterData.inventory = inventory;
                }
                current.OnEnemiesChanged += (enemies) => _enemies = enemies;
                current.BroadcastMessage();
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

