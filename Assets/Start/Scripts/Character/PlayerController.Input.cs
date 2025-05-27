using UnityEngine;
using Start.Scripts.Enemy;
using System.Collections.Generic;
using Start.Scripts.Map;
using System.Linq;



namespace Start.Scripts.Character
{
    public partial class PlayerController
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
                    _combatController.AttackOtherCharacter(this, _selectedEnemy.GetComponent<EnemyController>());
                }
            }
        }


        private bool EnemyInWeaponRange()
        {
            if (_selectedEnemy == null || _overlayTile == null) return false;
            var tilesInRange = GetTilesInRange(_overlayTile.Grid2DLocation, characterData.equippedWeapon.weaponRange);
            if (tilesInRange == null || tilesInRange.Count == 0) return false;
            // if the path is blocked return false
            if (tilesInRange.Any(tile => tile.isBlocked && !_selectedEnemy.standingOnTile.Grid2DLocation.Equals(tile.Grid2DLocation)))
            {
                return false;
            }
            if (tilesInRange.Any(tile => tile.Grid2DLocation.Equals(_selectedEnemy.standingOnTile.Grid2DLocation)))
            {
                return true;
            }
            return false;
        }


        public List<OverlayTile> GetTilesInRange(Vector2Int location, int range)
        {
            var startingTile = MapManager.Instance.Map[location];
            var inRangeTiles = new List<OverlayTile>();
            int stepCount = 0;

            inRangeTiles.Add(startingTile);

            //Should contain the surroundingTiles of the previous step. 
            var tilesForPreviousStep = new List<OverlayTile> { startingTile };
            while (stepCount < range)
            {
                var surroundingTiles = new List<OverlayTile>();

                foreach (var item in tilesForPreviousStep)
                {
                    if (range > 1)
                    {
                        surroundingTiles.AddRange(MapManager.Instance.GetSurroundingTiles
                            (new Vector2Int(item.gridLocation.x, item.gridLocation.y)));
                        continue;
                    }
                    surroundingTiles.AddRange(MapManager.Instance.GetAllSurroundingTiles
                        (new Vector2Int(item.gridLocation.x, item.gridLocation.y)));
                }

                inRangeTiles.AddRange(surroundingTiles);
                tilesForPreviousStep = surroundingTiles.Distinct().ToList();
                stepCount++;
            }

            return inRangeTiles.Distinct().ToList();
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

