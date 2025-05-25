using UnityEngine;
using Start.Scripts.Map;
using Start.Scripts.Enemy;

namespace Start.Scripts.Character
{
    public partial class PlayerController : MonoBehaviour
    {
        private void Update()
        {
            if (_combatController == null || !_combatController.isTurn) return;

            GetInRangeTiles();
            HandleCursorMovement();

            if (hasMovement)
            {
                HandleMovement();
            }
            else if (hasAttack)
            {
                HandleAttack();
            }
        }

        private void HandleCursorMovement()
        {
            var hit = GetFocusedOnTile();
            if (hit.HasValue && hit.Value.collider != null && hit.Value.collider.gameObject.CompareTag("OverlayTile"))
            {
                _overlayTile = hit.Value.collider.gameObject.GetComponent<OverlayTile>();
                if (Cursor != null && _overlayTile != null)
                {
                    Cursor.transform.position = _overlayTile.transform.position;
                }
            }
        }

        private void HandleAttack()
        {
            var hit = GetFocusedOnTile();
            if (hit.HasValue && hit.Value.collider != null && hit.Value.collider.gameObject.CompareTag("enemy"))
            {
                _selectedEnemy = hit.Value.collider.gameObject.GetComponent<EnemyController>();
                if (Cursor != null && _selectedEnemy != null)
                {
                    Cursor.transform.position = _selectedEnemy.transform.position + Vector3.up;
                }

                if (Input.GetMouseButtonDown(0) && _selectedEnemy && EnemyInWeaponRange())
                {
                    AttackEnemy();
                }
            }
        }

        private static RaycastHit2D? GetFocusedOnTile()
        {
            if (Camera.main == null) return null;

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos2D, Vector2.zero);
            return hits.Length > 0 ? hits[0] : (RaycastHit2D?)null;
        }
    }
}

