// InputRouter.cs

using UnityEngine;
using Start.Scripts.BaseClasses;
using Start.Scripts.Character;
namespace Start.Scripts.Game
{
    public class InputRouter : MonoBehaviour
    {
        private GameManager _gameManager;
        private OverlayTile _overlayTile;
        private GameObject _cursor;
        private void Start()
        {
            _gameManager = GameManager.Instance;
            _cursor = _gameManager.Cursor;
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
            _gameManager.TogglePause();
        }

        private void HandleConfirm()
        {
            // Future: Send confirm event to currently focused UI or system
            Debug.Log("Confirm Input Received");
            _gameManager.Confirm();
        }

        private void HandleCancel()
        {
            // Future: Close menus, cancel actions
            Debug.Log("Cancel Input Received");
            _gameManager.Cancel();
        }

        private void HandleMoveInput()
        {
            // Could be passed to PlayerController or UI navigation
        }

        private void HandleMouseMoved(Vector2 position)
        {
            // Optional: update hover states, cursors, etc.
            var tile = GetFocusedOnTile();
            if (tile != null)
            {
                Debug.Log($"Tile: {tile.Value}");
            }
        }

        private void HandleMouseMoved()
        {
            var hit = GetFocusedOnTile();
            if (hit.HasValue && hit.Value.collider != null && hit.Value.collider.gameObject.CompareTag("OverlayTile"))
            {
                _overlayTile = hit.Value.collider.gameObject.GetComponent<OverlayTile>();
                if (_cursor != null && _overlayTile != null)
                {
                    _cursor.transform.position = _overlayTile.transform.position;
                }
            }
        }

        private static RaycastHit2D? GetFocusedOnTile()
        {
            if (Camera.main == null) return null;

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos2D, Vector2.zero);
            return hits.Length > 0 ? hits[0] : null;
        }

        private void HandlePrimaryClick()
        {
            // Used by selection systems or PlayerController
            Debug.Log("Primary Mouse Click");
            _gameManager.Confirm();
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

