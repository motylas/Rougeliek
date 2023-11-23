using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Player player;
    public DungeonCreator dungeonCreator;
    public int maxEnemies = 8;
    public int minEnemies = 5;
    [UDictionary.Split(30, 70)] public SpellsDict spellsDict;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }
    
    [Serializable] public class SpellsDict : UDictionary<string, Spells> {}
    public enum RoomType
    {
        Starting,
        Normal,
        Boss
    }
}