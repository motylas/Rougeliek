using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
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
    [SerializeField] private Transform enemiesContainer;
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
        var boss = Instantiate(enemies[0], transform.position, quaternion.identity, enemiesContainer);
        boss.gameObject.transform.localScale *= 2;
        boss.GetDamage(-15);
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
