using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Player player;
    public Room currentRoom;
    public DungeonCreator dungeonCreator;
    public List<Transform> enemiesList = new();
    public List<SpriteRenderer> projectilesList = new();
    public int maxEnemies = 8;
    public int minEnemies = 5;

    public int keys;
    public TextMeshProUGUI keysTextMeshPro;
    public TextMeshProUGUI healthTextMeshPro;
    public TextMeshProUGUI strengthTextMeshPro;
    public Image currentWeaponImage;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    public void SetCurrentRoom(Room room)
    {
        currentRoom.blackMask.enabled = true;
        currentRoom = room;
    }
    
    public enum RoomType
    {
        Starting,
        Normal,
        Boss
    }
}