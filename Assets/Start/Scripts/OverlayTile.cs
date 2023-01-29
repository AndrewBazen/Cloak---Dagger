using System;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Serialization;

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
        public Vector2Int Grid2DLocation => new (gridLocation.x, gridLocation.y);

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
    }
}
