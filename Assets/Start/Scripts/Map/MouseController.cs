using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Start.Scripts.Map;
using Start.Scripts.Game;
using Start.Scripts.Character;

namespace Start.Scripts
{
    public class MouseController : MonoBehaviour
    {

        public GameObject cursor;
        public float speed;
        public GameObject playerPrefab;
        private PlayerController _player;
        private bool _isPlayersTurn;
        public int playerMovement;

        private List<OverlayTile> _path;
        private List<OverlayTile> _rangeFinderTiles;
        private bool _isMoving;
        private static Camera _camera;
        private GameManager _gameManager;

        private void Start()
        {
            _gameManager = GameManager.Instance;
            _camera = Camera.main;
            _path = new List<OverlayTile>();
            _isMoving = false;
            _rangeFinderTiles = new List<OverlayTile>();

        }

        void Update()
        {
            RaycastHit2D? hit = GetFocusedOnTile();

            if (hit.HasValue)
            {
                OverlayTile tile = hit.Value.collider.gameObject.GetComponent<OverlayTile>();
                cursor.transform.position = tile.transform.position;
                if (_rangeFinderTiles.Contains(tile) && !_isMoving)
                {
                    _path = _gameManager.PathFinder.FindPath(_player.StandingOnTile, tile, _rangeFinderTiles, false);

                    foreach (var currTile in _path)
                    {
                        if (currTile != tile)
                            currTile.SetSprite();
                    }
                    tile.ShowTile();
                }
                MoveCharacter(tile);
            }
        }


        private void ResetTiles()
        {
            foreach (var overlayTile in MapManager.Instance.Map.Values)
            {
                if (!_rangeFinderTiles.Contains(overlayTile) && !_path.Contains(overlayTile))
                {
                    overlayTile.HideTile();
                }

                if (!_path.Contains(overlayTile))
                    overlayTile.ResetSprite();
            }
        }

        private void MoveCharacter(OverlayTile tile)
        {
            if (Input.GetMouseButtonDown(0))
            {
                tile.ShowTile();

                if (_player != null && tile != _player.StandingOnTile && _rangeFinderTiles.Contains(tile))
                {
                    _isMoving = true;
                }
            }
            ResetTiles();

            if (_path.Count > 0 && _isMoving)
            {
                MoveAlongPath();
            }
        }

        private void MoveAlongPath()
        {
            var step = speed * Time.deltaTime;


            float zIndex = _path[0].transform.position.z;

            _player.transform.position = Vector2.MoveTowards(_player.transform.position,
              _path[0].transform.position, step);
            _player.transform.position = new Vector3(_player.transform.position.x, _player.transform.position.y, zIndex);

            if (Vector2.Distance(_player.transform.position, _path[0].transform.position) < 0.00001f)
            {
                PositionCharacterOnLine(_path[0]);
                _path.RemoveAt(0);
            }

            if (_path.Count == 0 && _player != null)
            {
                GetInRangeTiles();
                _isMoving = false;
            }

        }

        private void PositionCharacterOnLine(OverlayTile tile)
        {
            var tilePos = tile.transform.position;
            _player.transform.position = new Vector3(tilePos.x, tilePos.y + 0.0001f, tilePos.z);
            _player.StandingOnTile = tile;
        }

        private static RaycastHit2D? GetFocusedOnTile()
        {
            Vector3 mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos2D, Vector2.zero);

            if (hits.Length > 0)
            {
                return hits.OrderByDescending(i => i.collider.transform.position.z).First();
            }
            return null;
        }

        public void GetInRangeTiles()
        {
            _rangeFinderTiles = _gameManager.RangeFinder.GetTilesInRange(new Vector2Int(_player.StandingOnTile.gridLocation.x,
              _player.StandingOnTile.gridLocation.y), playerMovement);

        }
    }
}
