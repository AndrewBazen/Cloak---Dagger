using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SuperTiled2Unity;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.TextCore.Text;
using Random = System.Random;


namespace Start.Scripts
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private float speed;
        [SerializeField] private EnemyData data;
        [SerializeField] private int movement;
        [SerializeField] public int initiative;




        private GameObject combatSystem;
        private CombatController _combatController;
        private GameObject _cursor;
        private PathFinder _pathFinder;
        private RangeFinder _rangeFinder;
        private CharacterInfo _characterInfo;
        private String _enemyType;
        private List<OverlayTile> _path;
        private List<OverlayTile> _rangeFinderTiles;
        private List<OverlayTile> _rangeTileDistances;
        private bool _isMoving;
        public bool isEnemysTurn;
        private int _counter;

        private GameObject player;

        private void Awake()
        {
            _characterInfo = GetComponent<CharacterInfo>();
        }

        // Start is called before the first frame update
        void Start()
        {
            combatSystem = GameObject.FindGameObjectWithTag("combatSystem");
            _combatController = combatSystem.GetComponent<CombatController>();
            _cursor = GameObject.FindGameObjectWithTag("cursor");
            _counter = 0;
            SetEnemyValues();
            isEnemysTurn = false;
            _pathFinder = new PathFinder();
            _rangeFinder = new RangeFinder();
            _path = new List<OverlayTile>();
            _isMoving = false;
            _rangeFinderTiles = new List<OverlayTile>();
            
        }

        // Update is called once per frame
        void LateUpdate()
        {
            _combatController.StartTurn();
            if (isEnemysTurn)
            {
                player = GameObject.FindGameObjectWithTag("Player");
                GetInRangeTiles();
                var playerInRange = GetPlayerRange();
                var playerTile = player.GetComponent<CharacterInfo>().standingOnTile;

                if (!playerInRange && !_isMoving)
                {
                    _path = _pathFinder.FindPath(_characterInfo.standingOnTile,
                        playerTile, new List<OverlayTile>());

                }else if (playerInRange && !_isMoving)
                {
                    if (_enemyType == "melee")
                    {
                        _path = _pathFinder.FindPath(_characterInfo.standingOnTile,
                                                    playerTile, _rangeFinderTiles);
                    }
                }
                MoveCharacter(playerTile);
            }
        }

        private bool GetPlayerRange()
        {
            var playerLocation = player.GetComponent<CharacterInfo>().standingOnTile;
            if (_rangeFinderTiles.Contains(playerLocation))
            {
                return true;
            }

            return false;
        }
        

        private void MoveCharacter(OverlayTile tile)
        {
            if (gameObject != null && tile != _characterInfo.standingOnTile)
            {
                _isMoving = true;
            }
            ResetTiles();
            if (_path.Count > 0 && _isMoving && _counter <= data.movement)
            {
                MoveAlongPath();
            }
            else
            {
                GetInRangeTiles();
                _isMoving = false;
                _combatController.StopTurn();
                _counter = 0;
                
            }
        }

        private void MoveAlongPath()
        {
            var step = speed * Time.deltaTime;
      

            float zIndex = _path[0].transform.position.z;
      
            _characterInfo.transform.position = Vector2.MoveTowards(_characterInfo.transform.position, _path[0].transform.position, step);
            _characterInfo.transform.position = new Vector3(_characterInfo.transform.position.x, _characterInfo.transform.position.y, zIndex);

            if (Vector2.Distance(_characterInfo.transform.position, _path[0].transform.position) < 0.00001f)
            {
                PositionEnemyOnLine(_path[0]);
                _path.RemoveAt(0);
                _counter++;
            }

            

            if ((_path.Count == 0 &&  gameObject != null) || (_path.Count - data.movement == 0 && gameObject != null))
            {
                GetInRangeTiles();
                _isMoving = false;
                _combatController.StopTurn();
                _counter = 0;
                
               
            }
        }

        private void ResetTiles()
        {
            foreach (var overlayTile in MapManager.Instance.Map.Values)
            {
                if (!_rangeFinderTiles.Contains(overlayTile) && !_path.Contains(overlayTile) 
                                                             && overlayTile != _characterInfo.standingOnTile)
                {
                    overlayTile.HideTile();
                }

                if (!_path.Contains(overlayTile))
                    overlayTile.ResetSprite();
            }
        }

        private void PositionEnemyOnLine(OverlayTile tile)
        {
            var tilePos = tile.transform.position;
            transform.position = new Vector3(tilePos.x, tilePos.y + 0.0001f, tilePos.z);
            _characterInfo.standingOnTile = tile;
        }

        private void GetInRangeTiles()
        {
            _rangeFinderTiles = _rangeFinder.GetTilesInRange(
                new Vector2Int(_characterInfo.standingOnTile.gridLocation.x, 
                    _characterInfo.standingOnTile.gridLocation.y), movement);
      
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.CompareTag("OverlayTile"))
            {
                 _characterInfo.standingOnTile = col.gameObject.GetComponent<OverlayTile>();
                 _characterInfo.standingOnTile.ShowTile();
            }
        }

        private void SetEnemyValues()
        {
            movement = data.movement;
            speed = data.speed;
            RollInitiative();
            data.initiative = initiative;
        }

        private void RollInitiative()
        {
            Random rand = new Random();
            initiative = rand.Next(1, 20);
        }
    }
}
