using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;


public class Room : MonoBehaviour
{
    [Header("Tilemaps")] 
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap wallsTilemap;
    [SerializeField] public Tilemap rocksTilemap;
    [SerializeField] public Tilemap boxesTilemap;
    [SerializeField] private Tilemap bushesTilemap;
    [SerializeField] private Tilemap collisionTilemap;
    [SerializeField] private Tilemap projectilesBlockTilemap;

    [Header("Tiles")] 
    [Header("Floor")] 
    [SerializeField] private Tile floorTile;

    [Header("Corners")] 
    [SerializeField] private Tile[] cornerUpLeft;
    [SerializeField] private Tile[] cornerUpRight;
    [SerializeField] private Tile[] cornerDownLeft;
    [SerializeField] private Tile[] cornerDownRight;

    [Header("Walls")] 
    [SerializeField] private Tile[] wallUp;
    [SerializeField] private Tile[] wallRight;
    [SerializeField] private Tile[] wallLeft;
    [SerializeField] private Tile[] wallDown;

    [Header("Vision Variables")] 
    [SerializeField] private Transform obstaclesContainer;
    [SerializeField] private SpriteRenderer bushPrefab;
    [SerializeField] private SpriteRenderer rockPrefab1;
    [SerializeField] private SpriteRenderer rockPrefab2;
    [SerializeField] private SpriteRenderer boxPrefab;
    [SerializeField] private SpriteRenderer floorPrefab;
    [SerializeField] private SpriteRenderer blackObstaclePrefab;
    [SerializeField] private TileBase[] tiles; // 0 - box, 1,2 - rocks, 3 - bush
    [HideInInspector] public List<SpriteRenderer> bushesList = new();
    [HideInInspector] public List<SpriteRenderer> rocksList = new();
    [HideInInspector] public List<SpriteRenderer> boxesList = new();
    [HideInInspector] public List<SpriteRenderer> floorsList = new();
    [HideInInspector] public List<SpriteRenderer> wallsList = new();


    [Header("Animation")] 
    [SerializeField] private string topDoorOpenAnim;
    [SerializeField] private string bottomDoorOpenAnim;
    [SerializeField] private string leftDoorOpenAnim;
    [SerializeField] private string rightDoorOpenAnim;
    [SerializeField] private string topDoorCloseAnim, bottomDoorCloseAnim, leftDoorCloseAnim, rightDoorCloseAnim;
    [SerializeField] private RuntimeAnimatorController topDoorAnimCon, bottomDoorAnimCon, leftDoorAnimCon, rightDoorAnimCon;

    private Animator topDoorAnim, bottomDoorAnim, leftDoorAnim, rightDoorAnim;
    
    [Header("Chest and Key")]
    [SerializeField] private Chest chestPrefab;
    [SerializeField] private double chestDropRate;


    [Header("Others")] 
    [SerializeField] private GameObject doorPrefab;
    [SerializeField] private float perlinNoiseScale = 20f;
    public Transform firstInnerCellTransform;
    public Transform enemiesContainer;
    [SerializeField] private Enemy[] enemies;
    [SerializeField] private Enemy boss;
    public SpriteRenderer blackMask;

    private RoomTemplate _roomTemplate;

    [HideInInspector] public int innerGridWidth;
    [HideInInspector] public int innerGridHeight;
    private readonly List<Vector3Int> gatesList = new();
    private const int TilemapsCount = 3; // 3 - there are 3 types of tilemaps, need to change if 
    private bool _visited = false;

    public void CreateFromTemplate(RoomTemplate roomTemplate)
    {
        _roomTemplate = roomTemplate;
        innerGridHeight = _roomTemplate.height - 6;
        innerGridWidth = _roomTemplate.width - 6;
        firstInnerCellTransform.position = new Vector2(_roomTemplate.position.x + 3, _roomTemplate.position.y - 3);
        blackMask.transform.localScale = new Vector2(_roomTemplate.width, _roomTemplate.height);
        PlaceFloor();
        PlaceCorners();
        PlaceWalls();
        PlaceDoors();
        PlaceProjectileBlockWalls();
        LockGates(false);
        if (_roomTemplate.roomType == GameManager.RoomType.Normal) PlaceObstacles();
        if (_roomTemplate.roomType == GameManager.RoomType.Starting)
        {
            blackMask.enabled = false;
            GameManager.Instance.currentRoom = this;
            GameManager.Instance.player.transform.position = transform.position;
            _visited = true;
        }
    }

    private void PlaceFloor()
    {
        var startingPosition = _roomTemplate.position;
        var startingCell = floorTilemap.WorldToCell(startingPosition);
        var startingCellX = startingCell.x;
        var currentCell = startingCell;
        for (var i = 0; i < _roomTemplate.height; i++)
        {
            for (var j = 0; j < _roomTemplate.width; j++)
            {
                floorTilemap.SetTile(currentCell, floorTile);
                currentCell.x++;
            }

            currentCell.x = startingCellX;
            currentCell.y--;
        }
    }

    private void PlaceCorners()
    {
        var startingPosition = _roomTemplate.position;
        var startingCell = wallsTilemap.WorldToCell(startingPosition);

        // Up Left
        var currentCell = startingCell;
        var startingCellX = startingCell.x;
        for (var i = 0; i < 3; i++)
        {
            for (var j = 0; j < 3; j++)
            {
                wallsTilemap.SetTile(currentCell, cornerUpLeft[3 * i + j]);
                collisionTilemap.SetTile(currentCell, cornerUpLeft[3 * i + j]);
                var obstaclePosition = wallsTilemap.CellToWorld(currentCell);
                obstaclePosition.x += .5f;
                obstaclePosition.y += .5f;
                wallsList.Add(Instantiate(blackObstaclePrefab, obstaclePosition, Quaternion.identity, obstaclesContainer));
                currentCell.x++;
            }

            currentCell.x = startingCellX;
            currentCell.y--;
        }

        // Up Right
        currentCell = startingCell;
        currentCell.x += _roomTemplate.width - 3;
        startingCellX = currentCell.x;
        for (var i = 0; i < 3; i++)
        {
            for (var j = 0; j < 3; j++)
            {
                wallsTilemap.SetTile(currentCell, cornerUpRight[3 * i + j]);
                collisionTilemap.SetTile(currentCell, cornerUpRight[3 * i + j]);
                var obstaclePosition = wallsTilemap.CellToWorld(currentCell);
                obstaclePosition.x += .5f;
                obstaclePosition.y += .5f;
                wallsList.Add(Instantiate(blackObstaclePrefab, obstaclePosition, Quaternion.identity, obstaclesContainer));
                currentCell.x++;
            }

            currentCell.x = startingCellX;
            currentCell.y--;
        }

        // Down Left
        currentCell = startingCell;
        currentCell.y -= _roomTemplate.height - 3;
        startingCellX = currentCell.x;
        for (var i = 0; i < 3; i++)
        {
            for (var j = 0; j < 3; j++)
            {
                wallsTilemap.SetTile(currentCell, cornerDownLeft[3 * i + j]);
                collisionTilemap.SetTile(currentCell, cornerDownLeft[3 * i + j]);
                var obstaclePosition = wallsTilemap.CellToWorld(currentCell);
                obstaclePosition.x += .5f;
                obstaclePosition.y += .5f;
                wallsList.Add(Instantiate(blackObstaclePrefab, obstaclePosition, Quaternion.identity, obstaclesContainer));
                currentCell.x++;
            }

            currentCell.x = startingCellX;
            currentCell.y--;
        }

        // Down Right
        currentCell = startingCell;
        currentCell.x += _roomTemplate.width - 3;
        currentCell.y -= _roomTemplate.height - 3;
        startingCellX = currentCell.x;
        for (var i = 0; i < 3; i++)
        {
            for (var j = 0; j < 3; j++)
            {
                wallsTilemap.SetTile(currentCell, cornerDownRight[3 * i + j]);
                collisionTilemap.SetTile(currentCell, cornerDownRight[3 * i + j]);
                var obstaclePosition = wallsTilemap.CellToWorld(currentCell);
                obstaclePosition.x += .5f;
                obstaclePosition.y += .5f;
                wallsList.Add(Instantiate(blackObstaclePrefab, obstaclePosition, Quaternion.identity, obstaclesContainer));
                currentCell.x++;
            }

            currentCell.x = startingCellX;
            currentCell.y--;
        }
    }

    private void PlaceProjectileBlockWalls()
    {
        var startingPosition = _roomTemplate.position;
        var startingCell = projectilesBlockTilemap.WorldToCell(startingPosition);

        // Up
        var currentCell = startingCell;
        for (var i = 0; i < _roomTemplate.width; i++)
        {
            projectilesBlockTilemap.SetTile(currentCell, wallUp[0]);
            currentCell.x++;
        }
        
        // Down
        currentCell = startingCell;
        currentCell.y -= _roomTemplate.height - 1;
        for (var i = 0; i < _roomTemplate.width; i++)
        {
            projectilesBlockTilemap.SetTile(currentCell, wallDown[0]);
            currentCell.x++;
        }
        
        // Left
        currentCell = startingCell;
        for (var i = 0; i < _roomTemplate.height; i++)
        {
            projectilesBlockTilemap.SetTile(currentCell, wallLeft[0]);
            currentCell.y--;
        }
        
        // Right
        currentCell = startingCell;
        currentCell.x += _roomTemplate.width - 1;
        for (var i = 0; i < _roomTemplate.height; i++)
        {
            projectilesBlockTilemap.SetTile(currentCell, wallRight[0]);
            currentCell.y--;
        }
    }

    private void PlaceWalls()
    {
        var startingPosition = _roomTemplate.position;
        var startingCell = wallsTilemap.WorldToCell(startingPosition);


        // Up
        var currentCell = startingCell;
        currentCell.x += 3;
        var startingCellX = currentCell.x;
        for (var i = 0; i < 3; i++)
        {
            for (var j = 3; j < _roomTemplate.width - 3; j++)
            {
                wallsTilemap.SetTile(currentCell, wallUp[(j + 1) % 2 + 2 * i]);
                collisionTilemap.SetTile(currentCell, wallUp[(j + 1) % 2 + 2 * i]);
                var obstaclePosition = wallsTilemap.CellToWorld(currentCell);
                obstaclePosition.x += .5f;
                obstaclePosition.y += .5f;
                wallsList.Add(Instantiate(blackObstaclePrefab, obstaclePosition, Quaternion.identity, obstaclesContainer));
                currentCell.x++;
            }

            currentCell.x = startingCellX;
            currentCell.y--;
        }

        // Down
        currentCell = startingCell;
        currentCell.x += 3;
        currentCell.y -= _roomTemplate.height - 3;
        startingCellX = currentCell.x;
        for (var i = 0; i < 3; i++)
        {
            for (var j = 3; j < _roomTemplate.width - 3; j++)
            {
                wallsTilemap.SetTile(currentCell, wallDown[(j + 1) % 2 + 2 * i]);
                collisionTilemap.SetTile(currentCell, wallDown[(j + 1) % 2 + 2 * i]);
                var obstaclePosition = wallsTilemap.CellToWorld(currentCell);
                obstaclePosition.x += .5f;
                obstaclePosition.y += .5f;
                wallsList.Add(Instantiate(blackObstaclePrefab, obstaclePosition, Quaternion.identity, obstaclesContainer));
                currentCell.x++;
            }

            currentCell.x = startingCellX;
            currentCell.y--;
        }

        // Left
        currentCell = startingCell;
        currentCell.y -= 3;
        startingCellX = currentCell.x;
        for (var i = 3; i < _roomTemplate.height - 3; i++)
        {
            for (var j = 0; j < 3; j++)
            {
                wallsTilemap.SetTile(currentCell, wallLeft[(i % 2) * 3 + j]);
                collisionTilemap.SetTile(currentCell, wallLeft[(i % 2) * 3 + j]);
                var obstaclePosition = wallsTilemap.CellToWorld(currentCell);
                obstaclePosition.x += .5f;
                obstaclePosition.y += .5f;
                wallsList.Add(Instantiate(blackObstaclePrefab, obstaclePosition, Quaternion.identity, obstaclesContainer));
                currentCell.x++;
            }

            currentCell.x = startingCellX;
            currentCell.y--;
        }

        // Right
        currentCell = startingCell;
        currentCell.x += _roomTemplate.width - 3;
        currentCell.y -= 3;
        startingCellX = currentCell.x;
        for (var i = 3; i < _roomTemplate.height - 3; i++)
        {
            for (var j = 0; j < 3; j++)
            {
                wallsTilemap.SetTile(currentCell, wallRight[(i % 2) * 3 + j]);
                collisionTilemap.SetTile(currentCell, wallRight[(i % 2) * 3 + j]);
                var obstaclePosition = wallsTilemap.CellToWorld(currentCell);
                obstaclePosition.x += .5f;
                obstaclePosition.y += .5f;
                wallsList.Add(Instantiate(blackObstaclePrefab, obstaclePosition, Quaternion.identity, obstaclesContainer));
                currentCell.x++;
            }

            currentCell.x = startingCellX;
            currentCell.y--;
        }
    }

    private void PlaceDoors()
    {
        var startingPosition = _roomTemplate.position;
        var startingCell = wallsTilemap.WorldToCell(startingPosition);

        // Up
        if (_roomTemplate.up)
        {
            var doorPosition = wallsTilemap.CellToWorld(startingCell);
            doorPosition.x += _roomTemplate.width / 2f;
            doorPosition.y -= 0.9f;
            var door = Instantiate(doorPrefab, doorPosition, Quaternion.identity, transform);
            topDoorAnim = door.GetComponent<Animator>();
            topDoorAnim.runtimeAnimatorController = topDoorAnimCon;
            var boxCollider2D = gameObject.AddComponent<BoxCollider2D>();
            boxCollider2D.offset = new Vector2(boxCollider2D.offset.x, door.transform.localPosition.y - 2.1f - .5f);
            boxCollider2D.size *= 2;
            boxCollider2D.isTrigger = true;
            gatesList.Add(new Vector3Int(startingCell.x + _roomTemplate.width / 2 - 1, startingCell.y));
            gatesList.Add(new Vector3Int(startingCell.x + _roomTemplate.width / 2, startingCell.y));
            gatesList.Add(new Vector3Int(startingCell.x + _roomTemplate.width / 2 - 1, startingCell.y - 1));
            gatesList.Add(new Vector3Int(startingCell.x + _roomTemplate.width / 2, startingCell.y - 1));
            gatesList.Add(new Vector3Int(startingCell.x + _roomTemplate.width / 2 - 1, startingCell.y - 2));
            gatesList.Add(new Vector3Int(startingCell.x + _roomTemplate.width / 2, startingCell.y - 2));
        }

        // Down
        if (_roomTemplate.down)
        {
            var doorPosition = wallsTilemap.CellToWorld(startingCell);
            doorPosition.x += _roomTemplate.width / 2f;
            doorPosition.y -= _roomTemplate.height - 3 + 0.1f;
            var door = Instantiate(doorPrefab, doorPosition, Quaternion.identity, transform);
            bottomDoorAnim = door.GetComponent<Animator>();
            bottomDoorAnim.runtimeAnimatorController = bottomDoorAnimCon;
            door.transform.Rotate(0f, 0f, 180f);
            var boxCollider2D = gameObject.AddComponent<BoxCollider2D>();
            boxCollider2D.offset = new Vector2(boxCollider2D.offset.x, door.transform.localPosition.y + 2.1f + 1.7f);
            boxCollider2D.size *= 2;
            boxCollider2D.isTrigger = true;
            gatesList.Add(new Vector3Int(startingCell.x + _roomTemplate.width / 2 - 1,
                startingCell.y - _roomTemplate.height + 3));
            gatesList.Add(new Vector3Int(startingCell.x + _roomTemplate.width / 2,
                startingCell.y - _roomTemplate.height + 3));
            gatesList.Add(new Vector3Int(startingCell.x + _roomTemplate.width / 2 - 1,
                startingCell.y - 1 - _roomTemplate.height + 3));
            gatesList.Add(new Vector3Int(startingCell.x + _roomTemplate.width / 2,
                startingCell.y - 1 - _roomTemplate.height + 3));
            gatesList.Add(new Vector3Int(startingCell.x + _roomTemplate.width / 2 - 1,
                startingCell.y - 2 - _roomTemplate.height + 3));
            gatesList.Add(new Vector3Int(startingCell.x + _roomTemplate.width / 2,
                startingCell.y - 2 - _roomTemplate.height + 3));
        }

        // Left
        if (_roomTemplate.left)
        {
            var doorPosition = wallsTilemap.CellToWorld(startingCell);
            doorPosition.x += 1.9f;
            doorPosition.y -= _roomTemplate.height / 2 - 1;
            var door = Instantiate(doorPrefab, doorPosition, Quaternion.identity, transform);
            leftDoorAnim = door.GetComponent<Animator>();
            leftDoorAnim.runtimeAnimatorController = leftDoorAnimCon;
            door.transform.Rotate(0f, 0f, 90f);
            var boxCollider2D = gameObject.AddComponent<BoxCollider2D>();
            var localPosition = door.transform.localPosition;
            boxCollider2D.offset = new Vector2(localPosition.x + 2.1f + .5f, localPosition.y);
            boxCollider2D.size *= 2;
            boxCollider2D.isTrigger = true;
            gatesList.Add(new Vector3Int(startingCell.x, startingCell.y - _roomTemplate.height / 2));
            gatesList.Add(new Vector3Int(startingCell.x + 1, startingCell.y - _roomTemplate.height / 2));
            gatesList.Add(new Vector3Int(startingCell.x + 2, startingCell.y - _roomTemplate.height / 2));
            gatesList.Add(new Vector3Int(startingCell.x, startingCell.y - _roomTemplate.height / 2 + 1));
            gatesList.Add(new Vector3Int(startingCell.x + 1, startingCell.y - _roomTemplate.height / 2 + 1));
            gatesList.Add(new Vector3Int(startingCell.x + 2, startingCell.y - _roomTemplate.height / 2 + 1));
        }

        // Right
        if (_roomTemplate.right)
        {
            var doorPosition = wallsTilemap.CellToWorld(startingCell);
            doorPosition.x += _roomTemplate.width - 3 + 1.1f;
            doorPosition.y -= _roomTemplate.height / 2 - 1;
            var door = Instantiate(doorPrefab, doorPosition, Quaternion.identity, transform);
            rightDoorAnim = door.GetComponent<Animator>();
            rightDoorAnim.runtimeAnimatorController = rightDoorAnimCon;
            door.transform.Rotate(0f, 0f, -90f);
            var boxCollider2D = gameObject.AddComponent<BoxCollider2D>();
            var localPosition = door.transform.localPosition;
            boxCollider2D.offset = new Vector2(localPosition.x - 2.1f - .5f, localPosition.y);
            boxCollider2D.size *= 2;
            boxCollider2D.isTrigger = true;
            gatesList.Add(new Vector3Int(startingCell.x + _roomTemplate.width - 3,
                startingCell.y - _roomTemplate.height / 2));
            gatesList.Add(new Vector3Int(startingCell.x + 1 + _roomTemplate.width - 3,
                startingCell.y - _roomTemplate.height / 2));
            gatesList.Add(new Vector3Int(startingCell.x + 2 + _roomTemplate.width - 3,
                startingCell.y - _roomTemplate.height / 2));
            gatesList.Add(new Vector3Int(startingCell.x + _roomTemplate.width - 3,
                startingCell.y - _roomTemplate.height / 2 + 1));
            gatesList.Add(new Vector3Int(startingCell.x + 1 + _roomTemplate.width - 3,
                startingCell.y - _roomTemplate.height / 2 + 1));
            gatesList.Add(new Vector3Int(startingCell.x + 2 + _roomTemplate.width - 3,
                startingCell.y - _roomTemplate.height / 2 + 1));
        }
    }

    private void PlaceObstacles()
    {
        var tilesGrid = new bool[innerGridHeight, innerGridWidth];
        var startingCell = rocksTilemap.WorldToCell(firstInnerCellTransform.position);
        var startingX = startingCell.x;
        var offsetX = MyUtils.RandomFloat(0f, 99999f);
        var offsetY = MyUtils.RandomFloat(0f, 99999f);
        var currentCell = startingCell;
        // Place floor Vision
        for (var i = 0; i < innerGridHeight; i++)
        {
            for (var j = 0; j < innerGridWidth; j++)
            {
                var floorPosition = floorTilemap.CellToWorld(currentCell);
                floorPosition.x += .5f;
                floorPosition.y += .5f;
                floorsList.Add(Instantiate(floorPrefab, floorPosition, Quaternion.identity, obstaclesContainer));
                currentCell.x++;
            }

            currentCell.x = startingX;
            currentCell.y--;
        }

        startingCell.x++;
        startingCell.y--;
        startingX = startingCell.x;
        currentCell = startingCell;
        // Obstacles Logic
        for (var i = 1; i < innerGridHeight - 1; i++)
        {
            for (var j = 1; j < innerGridWidth - 1; j++)
            {
                var sample = PerlinNoise(i, j, offsetX, offsetY);
                if (sample > 0.75f)
                {
                    AddTileToProperTilemap(currentCell, tilesGrid, i, j);
                }

                currentCell.x++;
            }

            currentCell.x = startingX;
            currentCell.y--;
        }

        var unblockedTiles = new bool[innerGridHeight, innerGridWidth];
        CheckForBlockedArea(tilesGrid, 0, 0, unblockedTiles);
        RepairArea(tilesGrid, unblockedTiles, startingCell);
    }

    private void AddTileToProperTilemap(Vector3Int currentCell, bool[,] tilesGrid, int i, int j)
    {
        var tilemapType = MyUtils.Random.Next(TilemapsCount);
        Tile tile;
        switch (tilemapType)
        {
            case 0:
                tile = (Tile)tiles[0];
                tile.color = new Color(tile.color.r, tile.color.g, tile.color.b, 1);
                boxesTilemap.SetTile(currentCell, tile);
                tilesGrid[i, j] = true;
                var boxPosition = boxesTilemap.CellToWorld(currentCell);
                boxPosition.x += .5f;
                boxPosition.y += .5f;
                boxesList.Add(Instantiate(boxPrefab, boxPosition, Quaternion.identity, obstaclesContainer));
                break;
            case 1:
                var randomRock = MyUtils.Random.Next(2) + 1;
                tile = (Tile)tiles[randomRock];
                tile.color = new Color(tile.color.r, tile.color.g, tile.color.b, 1);
                rocksTilemap.SetTile(currentCell, tile);
                tilesGrid[i, j] = true;
                var rockPosition = rocksTilemap.CellToWorld(currentCell);
                rockPosition.x += .5f;
                rockPosition.y += .5f;
                rocksList.Add(randomRock == 1
                    ? Instantiate(rockPrefab1, rockPosition, Quaternion.identity, obstaclesContainer)
                    : Instantiate(rockPrefab2, rockPosition, Quaternion.identity, obstaclesContainer));
                break;
            case 2:
                tile = (Tile)tiles[3];
                tile.color = new Color(tile.color.r, tile.color.g, tile.color.b, 1);
                bushesTilemap.SetTile(currentCell, tile);
                var bushPosition = bushesTilemap.CellToWorld(currentCell);
                bushPosition.x += .5f;
                bushPosition.y += .5f;
                bushesList.Add(Instantiate(bushPrefab, bushPosition, Quaternion.identity, obstaclesContainer));
                break;
            default:
                Debug.LogError("Tilemap type index over bounds!");
                break;
        }
    }

    private float PerlinNoise(int x, int y, float offsetX, float offsetY)
    {
        var xCoord = (float)x / innerGridWidth * perlinNoiseScale + offsetX;
        var yCoord = (float)y / innerGridHeight * perlinNoiseScale + offsetY;
        return Mathf.PerlinNoise(xCoord, yCoord);
    }

    private void CheckForBlockedArea(bool[,] tilesGrid, int x, int y, bool[,] unblockedTiles)
    {
        unblockedTiles[x, y] = true;
        if (x != 0 && !tilesGrid[x - 1, y] && !unblockedTiles[x - 1, y])
            CheckForBlockedArea(tilesGrid, x - 1, y, unblockedTiles);
        if (x != innerGridHeight - 1 && !tilesGrid[x + 1, y] && !unblockedTiles[x + 1, y])
            CheckForBlockedArea(tilesGrid, x + 1, y, unblockedTiles);
        if (y != 0 && !tilesGrid[x, y - 1] && !unblockedTiles[x, y - 1])
            CheckForBlockedArea(tilesGrid, x, y - 1, unblockedTiles);
        if (y != innerGridWidth - 1 && !tilesGrid[x, y + 1] && !unblockedTiles[x, y + 1])
            CheckForBlockedArea(tilesGrid, x, y + 1, unblockedTiles);
    }

    private void RepairArea(bool[,] tilesGrid, bool[,] unlockedTiles, Vector3Int startingCell)
    {
        var rockTilesLength = tiles.Length;
        startingCell += Vector3Int.left + Vector3Int.up;
        var startingX = startingCell.x;
        for (var i = 0; i < innerGridHeight; i++)
        {
            for (var j = 0; j < innerGridWidth; j++)
            {
                if (!tilesGrid[i, j] && !unlockedTiles[i, j])
                    rocksTilemap.SetTile(startingCell, tiles[MyUtils.Random.Next(rockTilesLength)]);
                startingCell.x++;
            }

            startingCell.x = startingX;
            startingCell.y--;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        GameManager.Instance.SetCurrentRoom(this);
        HandleEnemies();
        blackMask.enabled = false;
    }

    private void HandleEnemies()
    {
        if (_visited) return;
        _visited = true;
        if (_roomTemplate.roomType == GameManager.RoomType.Normal) OnRoomNormal();
        else OnRoomBoss();
    }

    private void OnRoomNormal()
    {
        LockGates(true);
        SpawnEnemies();
        GameManager.Instance.player.OnStartVision();
        StartCoroutine(CheckForEnemies());
    }

    private void OnRoomBoss()
    {
        LockGates(true);
        SpawnBoss();
        StartCoroutine(CheckForEnemies());
    }

    private void LockGates(bool isLocked)
    {
        foreach (var gateTile in gatesList) collisionTilemap.SetTile(gateTile, isLocked ? floorTile : null);

        // Animation
        if (isLocked)
        {
            if (topDoorAnim) topDoorAnim.Play(topDoorCloseAnim);
            if (bottomDoorAnim) bottomDoorAnim.Play(bottomDoorCloseAnim);
            if (rightDoorAnim) rightDoorAnim.Play(rightDoorCloseAnim);
            if (leftDoorAnim) leftDoorAnim.Play(leftDoorCloseAnim);
        }
        else
        {
            if (topDoorAnim) topDoorAnim.Play(topDoorOpenAnim);
            if (bottomDoorAnim) bottomDoorAnim.Play(bottomDoorOpenAnim);
            if (rightDoorAnim) rightDoorAnim.Play(rightDoorOpenAnim);
            if (leftDoorAnim) leftDoorAnim.Play(leftDoorOpenAnim);
        }
    }

    private void SpawnEnemies()
    {
        var enemiesToSpawn = MyUtils.Random.Next(GameManager.Instance.minEnemies, GameManager.Instance.maxEnemies + 1);
        var value = innerGridHeight * innerGridHeight / (34f * 16f);
        enemiesToSpawn = (int) (value * enemiesToSpawn);
        if (enemiesToSpawn == 0) enemiesToSpawn = 5;
        var currentEnemies = 0;
        var playerPosition = GameManager.Instance.player.wallCollider.transform.position;
        while (currentEnemies < enemiesToSpawn)
        {
            Vector2 spawnPosition;
            do { spawnPosition = MyUtils.GetRandomPositionInRoom();} 
                while (Vector2.Distance(spawnPosition, playerPosition) < 3f);
            var enemyIndex = MyUtils.Random.Next(0, enemies.Length);
            var enemy = Instantiate(enemies[enemyIndex], spawnPosition, quaternion.identity, enemiesContainer);
            enemy.transform.position += enemy.transform.position - enemy.circleCollider2D.transform.position;
            currentEnemies++;
        }
    }

    private void SpawnBoss()
    {
        Instantiate(boss, transform.position, quaternion.identity, enemiesContainer);
    }

    private IEnumerator CheckForEnemies()
    {
        while (enemiesContainer.childCount != 0)
        {
            yield return new WaitForSeconds(0.1f);
        }

        OnRoomClear();
        if (_roomTemplate.roomType == GameManager.RoomType.Normal)
        {
            GameManager.Instance.player.OnEndVision();
            if (chestDropRate > MyUtils.Random.NextDouble()) Instantiate(chestPrefab, MyUtils.GetRandomPositionInRoom(), Quaternion.identity, transform);
        }
        if (_roomTemplate.roomType == GameManager.RoomType.Boss)
            Instantiate(chestPrefab, MyUtils.GetRandomPositionInRoom(), Quaternion.identity, transform);
    }
    

    private void OnRoomClear()
    {
        LockGates(false);
    }
}