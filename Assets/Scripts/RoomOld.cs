using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;


public class RoomOld : MonoBehaviour
{
    private bool _topRoom, _bottomRoom, _leftRoom, _rightRoom;
    [SerializeField] private BoxCollider2D topWallC, bottomWallC, leftWallC, rightWallC;
    [SerializeField] private SpriteRenderer topDoor, bottomDoor, leftDoor, rightDoor;
    [SerializeField] private BoxCollider2D topGateC, bottomGateC, leftGateC, rightGateC;
    [SerializeField] private Animator topDoorAnim, bottomDoorAnim, leftDoorAnim, rightDoorAnim;
    [SerializeField] private string topDoorOpenAnim, bottomDoorOpenAnim, leftDoorOpenAnim, rightDoorOpenAnim;
    [SerializeField] private string topDoorCloseAnim, bottomDoorCloseAnim, leftDoorCloseAnim, rightDoorCloseAnim;
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private Enemy[] enemies;
    [SerializeField] private Enemy boss;
    [SerializeField] public Transform enemiesContainer;
    [SerializeField] public Tilemap floorTilemap;
    [SerializeField] public Tilemap rocksTilemap;
    [SerializeField] public Tilemap boxesTilemap;
    [SerializeField] public Tilemap bushesTilemap;
    [SerializeField] private TileBase[] tiles; // 0 - box, 1,2 - rocks, 3 - bush
    [SerializeField] public Transform firstCellTransform;
    [SerializeField] public int gridWidth, gridHeight;
    [SerializeField] private float perlinNoiseScale = 20f;
    private GameManager.RoomType _roomType;
    private const int TilemapsCount = 3; // 3 - there are 3 types of tilemaps, need to change if 

    private Camera _camera;
    private bool _visited;

    [SerializeField] private Transform obstaclesContainer;
    [SerializeField] private SpriteRenderer bushPrefab;
    [SerializeField] private SpriteRenderer rockPrefab1;
    [SerializeField] private SpriteRenderer rockPrefab2;
    [SerializeField] private SpriteRenderer boxPrefab;
    [SerializeField] private SpriteRenderer floorPrefab;
    public List<SpriteRenderer> bushesList = new();
    public List<SpriteRenderer> rocksList = new();
    public List<SpriteRenderer> boxesList = new();
    public List<SpriteRenderer> floorsList = new();
    
    private void Start()
    {
        _camera = Camera.main;
        Invoke(nameof(OnRoomCreate), 0.05f);
    }

    private void OnRoomCreate()
    {
        // GenerateObstacles();
        if (!_topRoom)
        {
            topWallC.enabled = true;
            topDoor.enabled = false;
        }
        if (!_bottomRoom)
        {
            bottomWallC.enabled = true;
            bottomDoor.enabled = false;
        }
        if (!_leftRoom)
        {
            leftWallC.enabled = true;
            leftDoor.enabled = false;
        }
        if (!_rightRoom)
        {
            rightWallC.enabled = true;
            rightDoor.enabled = false;
        }
        if (_roomType == GameManager.RoomType.Normal) GenerateObstacles();
        // if (_roomType == GameManager.RoomType.Starting) GameManager.Instance.currentRoom = this;
    }

    private void GenerateObstacles()
    {
        var tilesGrid = new bool[gridHeight, gridWidth];
        var startingCell = rocksTilemap.WorldToCell(firstCellTransform.position);
        var startingX = startingCell.x;
        var offsetX = MyUtils.RandomFloat(0f, 99999f);
        var offsetY = MyUtils.RandomFloat(0f, 99999f);
        var currentCell = startingCell;
        // Place floor
        for (var i = 0; i < gridHeight; i++)
        {
            for (var j = 0; j < gridWidth; j++)
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
        for (var i = 1; i < gridHeight-1; i++)
        {
            for (var j = 1; j < gridWidth-1; j++)
            {
                var sample = PerlinNoise(i,j, offsetX, offsetY);
                if (sample > 0.75f)
                {
                    AddTileToProperTilemap(currentCell, tilesGrid, i, j);
                }
                currentCell.x++;
            }
            currentCell.x = startingX;
            currentCell.y--;
        }
        var unblockedTiles = new bool[gridHeight, gridWidth];
        CheckForBlockedArea(tilesGrid, 0,0, unblockedTiles);
        RepairArea(tilesGrid, unblockedTiles, startingCell);
    }

    private void AddTileToProperTilemap(Vector3Int currentCell, bool[,] tilesGrid, int i, int j)
    {
        var tilemapType = MyUtils.Random.Next(TilemapsCount);
        Tile tile;
        switch (tilemapType)
        {
            case 0:
                tile = (Tile) tiles[0];
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
                tile = (Tile) tiles[randomRock];
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
                tile = (Tile) tiles[3];
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
        var xCoord = (float) x / gridWidth * perlinNoiseScale + offsetX;
        var yCoord = (float) y / gridHeight * perlinNoiseScale + offsetY;
        return Mathf.PerlinNoise(xCoord, yCoord);
    }

    private void CheckForBlockedArea(bool[,] tilesGrid, int x, int y, bool[,] unblockedTiles)
    {
        unblockedTiles[x, y] = true;
        if (x != 0 && !tilesGrid[x - 1, y]  && !unblockedTiles[x-1,y]) CheckForBlockedArea(tilesGrid, x-1,y,unblockedTiles);
        if (x != gridHeight - 1 && !tilesGrid[x + 1, y] && !unblockedTiles[x+1,y]) CheckForBlockedArea(tilesGrid, x+1,y,unblockedTiles);
        if (y != 0 && !tilesGrid[x, y - 1] && !unblockedTiles[x,y-1]) CheckForBlockedArea(tilesGrid, x,y-1,unblockedTiles);
        if (y != gridWidth - 1 && !tilesGrid[x, y + 1] && !unblockedTiles[x, y + 1]) CheckForBlockedArea(tilesGrid, x, y + 1, unblockedTiles);
    }

    private void RepairArea(bool[,] tilesGrid, bool[,] unlockedTiles, Vector3Int startingCell)
    {
        var rockTilesLength = tiles.Length;
        startingCell += Vector3Int.left + Vector3Int.up;
        var startingX = startingCell.x;
        for (var i = 0; i < gridHeight; i++)
        {
            for (var j = 0; j < gridWidth; j++)
            {
                if (!tilesGrid[i,j] && !unlockedTiles[i,j]) rocksTilemap.SetTile(startingCell, tiles[MyUtils.Random.Next(rockTilesLength)]);
                startingCell.x++;
            }
            startingCell.x = startingX;
            startingCell.y--;
        }
    }

    public void SetType(GameManager.RoomType roomType)
    {
        _roomType = roomType;
        _visited = _roomType == GameManager.RoomType.Starting;
    }

    public void SetNeighbours(bool top, bool bottom, bool left, bool right)
    {
        _topRoom = top;
        _bottomRoom = bottom;
        _leftRoom = left;
        _rightRoom = right;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        // GameManager.Instance.currentRoom = this;
        HandleCamera();
        HandleEnemies();
    }
    
    private void HandleCamera()
    {
        var cameraTransform = _camera.transform;
        var thisPos = transform.position;

        cameraTransform.position = new Vector3(thisPos.x, thisPos.y, cameraTransform.position.z);
    }

    private void HandleEnemies()
    {
        if (_visited) return;
        _visited = true;
        if (_roomType == GameManager.RoomType.Normal) OnRoomNormal();
        else OnRoomBoss();
    }

    private void OnRoomNormal()
    {
        LockGates(true);
        Debug.Log("SPAWNING ENEMIES");
        SpawnEnemies();
        GameManager.Instance.player.OnStartVision();
        StartCoroutine(CheckForEnemies());
    }
    
    private void OnRoomBoss()
    {
        LockGates(true);
        Debug.Log("SPAWNING BOSS");
        SpawnBoss();
        StartCoroutine(CheckForEnemies());
    }

    private void LockGates(bool isLocked)
    {
        topGateC.enabled = isLocked;
        leftGateC.enabled = isLocked;
        rightGateC.enabled = isLocked;
        bottomGateC.enabled = isLocked;
        if (isLocked)
        {
            topDoorAnim.Play(topDoorCloseAnim);
            bottomDoorAnim.Play(bottomDoorCloseAnim);
            rightDoorAnim.Play(rightDoorCloseAnim);
            leftDoorAnim.Play(leftDoorCloseAnim);
        }
        else
        {
            topDoorAnim.Play(topDoorOpenAnim);
            bottomDoorAnim.Play(bottomDoorOpenAnim);
            rightDoorAnim.Play(rightDoorOpenAnim);
            leftDoorAnim.Play(leftDoorOpenAnim);
        }
    }

    private void SpawnEnemies()
    {
        var enemiesToSpawn = MyUtils.Random.Next(GameManager.Instance.minEnemies, GameManager.Instance.maxEnemies + 1);
        var currentEnemies = 0;
        while (currentEnemies < enemiesToSpawn)
        {
            var index = MyUtils.Random.Next(0, spawnPoints.Count);
            var enemyIndex = MyUtils.Random.Next(0, enemies.Length);
            Instantiate(enemies[enemyIndex], spawnPoints[index].position, quaternion.identity, enemiesContainer);
            spawnPoints.RemoveAt(index);
            currentEnemies++;
        }
        
        
        // TEST THAT SPAWN POINTS DOESNT CREATE STUCK ENEMIES
        // foreach (var spawnPoint in spawnPoints)
        // {
        //     Instantiate(enemies[0], spawnPoint.position, quaternion.identity, enemiesContainer);
        // }
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
        if (_roomType == GameManager.RoomType.Normal) GameManager.Instance.player.OnEndVision();
    }

    private void OnRoomClear()
    {
        LockGates(false);
    }
}
