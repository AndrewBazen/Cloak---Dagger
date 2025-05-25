// InputRouter.cs

using UnityEngine;

namespace Start.Scripts.Game
{
    public class InputRouter : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;

        private void Start()
        {
            if (InputManager.Instance == null)
            {
                Debug.LogError("InputManager not found in scene.");
                return;
            }

            InputManager.Instance.OnPausePressed += HandlePause;
            InputManager.Instance.OnConfirm += HandleConfirm;
            InputManager.Instance.OnCancel += HandleCancel;
            InputManager.Instance.OnMoveInput += HandleMoveInput;
            InputManager.Instance.OnMouseMoved += HandleMouseMoved;
            InputManager.Instance.OnPrimaryClick += HandlePrimaryClick;
        }

        private void HandlePause()
        {
            gameManager.TogglePause();
        }

        private void HandleConfirm()
        {
            // Future: Send confirm event to currently focused UI or system
            Debug.Log("Confirm Input Received");
        }

        private void HandleCancel()
        {
            // Future: Close menus, cancel actions
            Debug.Log("Cancel Input Received");
        }

        private void HandleMoveInput(Vector2 input)
        {
            // Could be passed to PlayerController or UI navigation
            Debug.Log($"Move Input: {input}");
        }

        private void HandleMouseMoved(Vector2 position)
        {
            // Optional: update hover states, cursors, etc.
        }

        private void HandlePrimaryClick()
        {
            // Used by selection systems or PlayerController
            Debug.Log("Primary Mouse Click");
        }

        private void OnDestroy()
        {
            if (InputManager.Instance == null) return;

            InputManager.Instance.OnPausePressed -= HandlePause;
            InputManager.Instance.OnConfirm -= HandleConfirm;
            InputManager.Instance.OnCancel -= HandleCancel;
            InputManager.Instance.OnMoveInput -= HandleMoveInput;
            InputManager.Instance.OnMouseMoved -= HandleMouseMoved;
            InputManager.Instance.OnPrimaryClick -= HandlePrimaryClick;
        }
    }
}

