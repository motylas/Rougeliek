using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

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
    [UDictionary.Split(30, 70)] public SpellsDict spellsDict;

    public int keys;
    public TextMeshProUGUI keysTextMeshPro;
    public TextMeshProUGUI healthTextMeshPro;
    public TextMeshProUGUI strengthTextMeshPro;

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
    
    [Serializable] public class SpellsDict : UDictionary<string, Spells> {}
    public enum RoomType
    {
        Starting,
        Normal,
        Boss
    }
}