using UnityEngine;

namespace Start.Scripts.Game
{
    /// <summary>
    /// Interface for components that need to interact with the GameManager.
    /// Implementing this interface indicates that the component will be initialized
    /// by the GameManager and can request services from it.
    /// </summary>
    public interface IGameManagerAware
    {
        /// <summary>
        /// Called by GameManager to initialize this component.
        /// </summary>
        /// <param name="gameManager">Reference to the GameManager</param>
        void Initialize(GameManager gameManager);
        
        /// <summary>
        /// Called by GameManager when the game state changes.
        /// </summary>
        /// <param name="newState">The new game state</param>
        void OnGameStateChanged(GameManager.GameState newState);
    }
} 