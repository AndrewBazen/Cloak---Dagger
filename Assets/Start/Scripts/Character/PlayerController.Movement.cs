using UnityEngine;
using Start.Scripts.Map;
using Start.Scripts.Interfaces;

namespace Start.Scripts.Character
{
    public partial class PlayerController
    {
        private void HandleMovement()
        {
            if (!_isMoving && _overlayTile != null && StandingOnTile != null)
            {
                _path = _gameManager.PathFinder.FindPath(StandingOnTile, _overlayTile, _rangeFinderTiles, false);
                HighlightPath();
            }

            MoveCharacter(_overlayTile);
        }

        private void MoveCharacter(OverlayTile tile)
        {
            if (Input.GetMouseButtonDown(0) && tile != null && !tile.isBlocked && _rangeFinderTiles.Contains(tile))
            {
                _isMoving = true;
                ResetTiles();
            }

            while (_path != null && _path.Count > 0 && _isMoving)
            {
                MoveAlongPath();
            }
        }

        private void MoveAlongPath()
        {
            var step = characterData.Speed * Time.deltaTime;
            var zIndex = _path[0].transform.position.z;

            transform.position = Vector2.MoveTowards(transform.position, _path[0].transform.position, step);
            transform.position = new Vector3(transform.position.x, transform.position.y, zIndex);

            if (Vector2.Distance(transform.position, _path[0].transform.position) < 0.00001f)
            {
                PositionCharacterOnTile(_path[0]);
                _path.RemoveAt(0);
            }

            if (_path.Count == 0)
            {
                _isMoving = false;
                characterData.HasMovement = false;
                _rangeFinderTiles = _gameManager.RangeFinder.GetRangeTiles(StandingOnTile.Grid2DLocation, characterData.Movement);
            }
        }

        private void PositionCharacterOnTile(OverlayTile tile)
        {
            if (tile == null) return;

            var tilePos = tile.transform.position;
            transform.position = new Vector3(tilePos.x, tilePos.y + 0.0001f, tilePos.z);
            StandingOnTile = tile;
        }

        private void HighlightPath()
        {
            if (_path == null) return;

            foreach (var tile in _path)
            {
                if (tile == null) continue;

                if (tile != _overlayTile)
                    tile.SetSprite();
                tile.ShowTile();
            }

            _overlayTile?.ShowTile();
        }

        private void ResetTiles()
        {
            if (MapManager.Instance == null || MapManager.Instance.Map == null) return;

            foreach (var overlayTile in MapManager.Instance.Map.Values)
            {
                if (overlayTile == null) continue;

                if (!_rangeFinderTiles.Contains(overlayTile) && !_path.Contains(overlayTile))
                {
                    overlayTile.HideTile();
                }
                if (!_path.Contains(overlayTile))
                {
                    overlayTile.ResetSprite();
                }
            }
        }
    }
}

