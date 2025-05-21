using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;


namespace Start.Scripts
{
  public class MouseController : MonoBehaviour
  {

    public GameObject cursor;
    public float speed;
    public GameObject playerPrefab;
    private CharacterInfo _player;
    private bool _isPlayersTurn;
    public int playerMovement;

    private PathFinder _pathFinder;
    private RangeFinder _rangeFinder;
    private List<OverlayTile> _path;
    private List<OverlayTile> _rangeFinderTiles;
    private bool _isMoving;
    private static Camera _camera;

    private void Start()
    {
      _camera = Camera.main;
      _pathFinder = new PathFinder();
      _rangeFinder = new RangeFinder();
      _path = new List<OverlayTile>();
      _isMoving = false;
      _rangeFinderTiles = new List<OverlayTile>();
      
    }

    void Update()
    {
      if (_player == null)
      {
        SpawnCharacter();
      }

      RaycastHit2D? hit = GetFocusedOnTile();

      if (hit.HasValue)
      {
        OverlayTile tile = hit.Value.collider.gameObject.GetComponent<OverlayTile>();
        cursor.transform.position = tile.transform.position;
        if (_rangeFinderTiles.Contains(tile) && !_isMoving)
        {
          _path = _pathFinder.FindPath(_player.standingOnTile, tile, _rangeFinderTiles, false);

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

    private void SpawnCharacter()
    {
      foreach (var tile in MapManager.Instance.Map.Values)
      {
        if (tile.isPlayerSpawnTile)
        {
          _player = Instantiate(playerPrefab).GetComponent<CharacterInfo>();
          PositionCharacterOnLine(tile);
          GetInRangeTiles();
        }
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

        if (_player != null && tile != _player.standingOnTile && _rangeFinderTiles.Contains(tile))
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

      if (_path.Count == 0 &&  _player != null)
      {
        GetInRangeTiles();
        _isMoving = false;
      }

    }

    private void PositionCharacterOnLine(OverlayTile tile)
    {
      var tilePos = tile.transform.position;
      _player.transform.position = new Vector3(tilePos.x, tilePos.y + 0.0001f, tilePos.z);
      _player.standingOnTile = tile;
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
      _rangeFinderTiles = _rangeFinder.GetTilesInRange(new Vector2Int(_player.standingOnTile.gridLocation.x,
        _player.standingOnTile.gridLocation.y), playerMovement);
      
    }
  }
}