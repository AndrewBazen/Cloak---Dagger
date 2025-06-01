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
                _gameManager.Party.RemoveFromParty(gameObject);
            }

            Destroy(gameObject);
        }

    }
}

