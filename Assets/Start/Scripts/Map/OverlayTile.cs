using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Unity.Collections;
using Unity.Mathematics;
using Start.Scripts.AI.Jobs;

namespace Start.Scripts
{
    public class OverlayTile : MonoBehaviour
    {
        [FormerlySerializedAs("G")] public int g;
        [FormerlySerializedAs("H")] public int h;
        public int F => g + h;

        private SpriteRenderer _spriteRenderer;
        public bool isPlayerSpawnTile;
        public bool isEnemySpawnTile;
        public bool isBlocked = false;
        public GameObject overlayPrefab;
        private SpriteRenderer _overlayRend;
        public Sprite pathSprite;
        public OverlayTile previous;
        public Vector3Int gridLocation;
        public Vector2Int Grid2DLocation => new(gridLocation.x, gridLocation.y);
        public List<OverlayTile> neighbors = new();

        private void Awake()
        {
            _overlayRend = overlayPrefab.GetComponent<SpriteRenderer>();
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        }

        public void HideTile()
        {
            _spriteRenderer.color = new Color(1, 1, 1, 0);
        }

        public void ShowTile()
        {
            _spriteRenderer.color = new Color(1, 1, 1, 1);
        }

        public void SetSprite()
        {
            _spriteRenderer.sprite = pathSprite;
            _spriteRenderer.color = new Color(1, 1, 1, 1);

        }

        public void ResetSprite()
        {
            _spriteRenderer.sprite = _overlayRend.sprite;
            _spriteRenderer.color = new Color(1, 1, 1, 0);
        }

        public GameObject GetPlayerOnTile()
        {
            var contacts = new List<Collider2D>();
            var col = gameObject.GetComponent<PolygonCollider2D>();
            col.GetContacts(contacts);
            if (contacts.Count != 0)
            {
                var playerOnTile = contacts[0].gameObject;
                return playerOnTile;
            }

            return null;
        }

        public List<OverlayTile> GetInRangeTiles(int range)
        {
            List<OverlayTile> inRangeTiles = new();
            foreach (var tile in MapManager.Instance.Map.Values)
            {
                if (tile == null) continue;

                int distance = Mathf.Abs(tile.gridLocation.x - gridLocation.x) +
                               Mathf.Abs(tile.gridLocation.y - gridLocation.y);
                if (distance <= range && !tile.isBlocked)
                {
                    inRangeTiles.Add(tile);
                }
            }

            return inRangeTiles;
        }


        // private void OnTriggerEnter2D(Collider2D col)
        // {
        //     isBlocked = true;
        // }
        //
        // private void OnTriggerExit2D(Collider2D other)
        // {
        //     isBlocked = false;
        // }
    }
}
