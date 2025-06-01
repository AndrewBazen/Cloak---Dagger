using UnityEngine;
using Start.Scripts.Enemy;
using System.Linq;

namespace Start.Scripts.Character
{
    public partial class PlayerController
    {
        private void Update()
        {
            if (_combatController == null || !_combatController.isTurn) return;

            _rangeFinderTiles = _gameManager.RangeFinder.GetRangeTiles(StandingOnTile.Grid2DLocation, characterData.Movement);
            HandleCursorMovement();

            if (characterData.HasMovement)
            {
                HandleMovement();
            }
            else if (characterData.HasAction && characterData.EquippedWeapon != null && characterData.EquippedWeapon.weaponRange > 0)
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
                    _combatController.AttackOtherCharacter(this, _selectedEnemy.GetComponent<EnemyController>());
                }
            }
        }


        private bool EnemyInWeaponRange()
        {
            if (_selectedEnemy == null || _overlayTile == null) return false;
            var tilesInRange = _gameManager.RangeFinder.GetRangeTiles(_overlayTile.Grid2DLocation, characterData.EquippedWeapon.weaponRange);
            if (tilesInRange == null || tilesInRange.Count == 0) return false;
            // if the path is blocked return false
            if (tilesInRange.Any(tile => tile.isBlocked && !_selectedEnemy.StandingOnTile.Grid2DLocation.Equals(tile.Grid2DLocation)))
            {
                return false;
            }
            if (tilesInRange.Any(tile => tile.Grid2DLocation.Equals(_selectedEnemy.StandingOnTile.Grid2DLocation)))
            {
                return true;
            }
            return false;
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

