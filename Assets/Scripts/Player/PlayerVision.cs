using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;


public class PlayerVision : MonoBehaviour
{
    [SerializeField] private float visionArea;
    private Room _currentRoom;
    private int _gridHeight;
    private int _gridWidth;
    private Vector3Int _startingCell;
    private Tilemap _tilemap;
    private Transform _firstCellTransform;
    private int _startingX;
    private Color _normalColor;
    private Color _seenColor;
    private Color _notSeenColor;

    private void Start()
    {
        Invoke(nameof(LateStart), .2f);
    }

    private void LateStart()
    {
        _currentRoom = GameManager.Instance.currentRoom;
        _gridHeight = _currentRoom.gridHeight;
        _gridWidth = _currentRoom.gridWidth;
        _tilemap = _currentRoom.floorTilemap;
        _firstCellTransform = _currentRoom.firstCellTransform;
        _startingCell = _tilemap.WorldToCell(_firstCellTransform.position);
        _startingX = _startingCell.x;
        _normalColor = _tilemap.GetTile<Tile>(_startingCell).color;
        _seenColor = new Color(0.2f, 0.2f, 0.2f, 1);
        _notSeenColor = new Color(0f, 0f, 0f, 1);

        StartCoroutine(FloorVision());
    }

    private IEnumerator FloorVision()
    {
        Blackout();
        while (true)
        {
            var currentCell = _startingCell;
            for (var i = 0; i < _gridHeight; i++)
            {
                for (var j = 0; j < _gridWidth; j++)
                {
                    var tilePos = _tilemap.CellToWorld(currentCell);
                    tilePos.x += 0.5f;
                    tilePos.y += 0.5f;
                    var distance = Vector2.Distance(transform.position, tilePos);

                    if (distance <= visionArea)
                    {
                        _tilemap.SetColor(currentCell, _normalColor);
                    }
                    else
                    {
                        if (_tilemap.GetColor(currentCell) == _normalColor) _tilemap.SetColor(currentCell, _seenColor);
                    }
                    
                    currentCell.x++;
                }

                currentCell.x = _startingX;
                currentCell.y--;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void Blackout()
    {
        var currentCell = _startingCell;
        for (var i = 0; i < _gridHeight; i++)
        {
            for (var j = 0; j < _gridWidth; j++)
            {
                _tilemap.SetTileFlags(currentCell, TileFlags.None);
                _tilemap.SetColor(currentCell, _notSeenColor);
                currentCell.x++;
            }
            currentCell.x = _startingX;
            currentCell.y--;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionArea); // Chase area draw
    }
}