using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


namespace Start.Scripts
{
  public class PlayerController : MonoBehaviour
  {

    public GameObject cursor;
    public float speed;
    public GameObject[] enemies;
    public GameObject playerPrefab;
    public CharacterInfo player;
    public bool isPlayersTurn;
    [SerializeField] private GameObject playerContainer;
    public int playerMovement;

    private readonly int _characterMovementRange = 6;
    
    private PathFinder _pathFinder;
    private RangeFinder _rangeFinder;
    private List<OverlayTile> _path;
    private List<OverlayTile> _rangeFinderTiles;
    private bool _isMoving;
    private static Camera _camera;

    private void Start()
    {
      isPlayersTurn = true;
      _camera = Camera.main;
      _pathFinder = new PathFinder();
      _rangeFinder = new RangeFinder();
      _path = new List<OverlayTile>();
      _isMoving = false;
      _rangeFinderTiles = new List<OverlayTile>();
      
    }

    void LateUpdate()
    {
      if (player == null)
      {
        SpawnCharacter();
      }

      enemies = GameObject.FindGameObjectsWithTag("enemy");

      if (isPlayersTurn)
      {
        OverlayTile tile = new OverlayTile();
        RaycastHit2D? hit = GetFocusedOnTile();

        if (hit.HasValue)
        {
          tile = hit.Value.collider.gameObject.GetComponent<OverlayTile>();
          cursor.transform.position = tile.transform.position;
          if (!_isMoving)
          {
            _path = _pathFinder.FindPath(player.standingOnTile, tile, _rangeFinderTiles);

            foreach (var currTile in _path)
            {
              if (currTile != tile)
                currTile.SetSprite();
            }

            tile.ShowTile();
          }
          
        }
        MoveCharacter(tile);
      }
    }

    private void SpawnCharacter()
    {
      player = Instantiate(playerPrefab, playerContainer.transform).GetComponent<CharacterInfo>();
      PositionCharacterOnLine(MapManager.Instance.playerSpawnTile);
      GetInRangeTiles();
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

        if (player != null && tile != player.standingOnTile && _rangeFinderTiles.Contains(tile))
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
      
      player.transform.position = Vector2.MoveTowards(player.transform.position, _path[0].transform.position, step);
      player.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, zIndex);

      if (Vector2.Distance(player.transform.position, _path[0].transform.position) < 0.00001f)
      {
        PositionCharacterOnLine(_path[0]);
        _path.RemoveAt(0);
      }

      if (_path.Count == 0 &&  player != null)
      {
        GetInRangeTiles();
        _isMoving = false;
        isPlayersTurn = false;
        foreach (var enemy in enemies)
        {
          enemy.GetComponent<EnemyController>().isEnemysTurn = true;
        }
      }

    }

    private void PositionCharacterOnLine(OverlayTile tile)
    {
      var tilePos = tile.transform.position;
      player.transform.position = new Vector3(tilePos.x, tilePos.y + 0.0001f, tilePos.z);
      player.standingOnTile = tile;
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
      _rangeFinderTiles = _rangeFinder.GetTilesInRange(new Vector2Int(player.standingOnTile.gridLocation.x, player.standingOnTile.gridLocation.y), _characterMovementRange);
      
    }
  }
}