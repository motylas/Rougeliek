using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;


public class Room : MonoBehaviour
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
    [SerializeField] public Tilemap rocksTilemap;
    [SerializeField] private TileBase[] rocksTiles;
    [SerializeField] public Transform firstCellTransform;
    [SerializeField] public int gridWidth, gridHeight;
    [SerializeField] private float perlinNoiseScale = 20f;
    private GameManager.RoomType _roomType;

    private Camera _camera;
    private bool _visited;
    
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
        if (_roomType == GameManager.RoomType.Starting) GameManager.Instance.currentRoom = this;
        GameManager.Instance.currentRoom = this; //TODO delete this line after testing
    }

    private void GenerateObstacles()
    {
        var tilesGrid = new bool[gridHeight, gridWidth];
        var startingCell = rocksTilemap.WorldToCell(firstCellTransform.position);
        startingCell.x++;
        startingCell.y--;
        var startingX = startingCell.x;
        var offsetX = MyUtils.RandomFloat(0f, 99999f);
        var offsetY = MyUtils.RandomFloat(0f, 99999f);
        var currentCell = startingCell;
        var rockTilesLength = rocksTiles.Length;
        for (var i = 1; i < gridHeight-1; i++)
        {
            for (var j = 1; j < gridWidth-1; j++)
            {
                var sample = PerlinNoise(i,j, offsetX, offsetY);
                if (sample > 0.75f)
                {
                    rocksTilemap.SetTile(currentCell, rocksTiles[MyUtils.Random.Next(rockTilesLength)]);
                    tilesGrid[i, j] = true;
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
        var rockTilesLength = rocksTiles.Length;
        startingCell += Vector3Int.left + Vector3Int.up;
        var startingX = startingCell.x;
        for (var i = 0; i < gridHeight; i++)
        {
            for (var j = 0; j < gridWidth; j++)
            {
                if (!tilesGrid[i,j] && !unlockedTiles[i,j]) rocksTilemap.SetTile(startingCell, rocksTiles[MyUtils.Random.Next(rockTilesLength)]);
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
    }

    private void OnRoomClear()
    {
        LockGates(false);
    }
}
