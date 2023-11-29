using System.Collections.Generic;
using UnityEngine;

public class DungeonCreator : MonoBehaviour
{
    private readonly List<Vector2> _availablePositions = new();
    private readonly List<Vector2> _takenPositions = new();
    private readonly List<Vector2> _unavailablePositions = new();
    [SerializeField] private int numberOfRooms;
    [SerializeField] private GameObject roomPrefab;
    public float horizontal, vertical;
    private readonly Dictionary<Vector2, bool> _takenPositionsMap = new();
    

    private void Start()
    {
        GenerateRoomsPositions();
        CreateRooms();
    }

    private void GenerateRoomsPositions()
    {
        var currentPosition = Vector2.zero;
        _availablePositions.Add(currentPosition);
        for (var i = 0; i < numberOfRooms; i++)
        {
            do
            {
                currentPosition = _availablePositions
                    [MyUtils.Random.Next(_availablePositions.Count)];
                _availablePositions.Remove(currentPosition);
            } while (_unavailablePositions.Contains(currentPosition));

            _takenPositions.Add(currentPosition);
            _unavailablePositions.Add(currentPosition);

            _takenPositionsMap.Add(currentPosition, true);
            
            AddVectorToLists(Vector2.left + currentPosition);
            AddVectorToLists(Vector2.right + currentPosition);
            AddVectorToLists(Vector2.up + currentPosition);
            AddVectorToLists(Vector2.down + currentPosition);
        }
    }

    private void AddVectorToLists(Vector2 vector2)
    {
        if (_unavailablePositions.Contains(vector2)) return;
        if (_availablePositions.Contains(vector2)) _unavailablePositions.Add(vector2);
        else _availablePositions.Add(vector2);
    }

    private void CreateRooms()
    {
        Room bossRoom = null;
        var bossRoomPos = Vector2.zero;
        foreach (var position in _takenPositions)
        {
            var roomGo = Instantiate(roomPrefab, new Vector2(position.x * horizontal, position.y * vertical), Quaternion.identity);
            var room = roomGo.GetComponent<Room>();
            room.SetNeighbours(_takenPositionsMap.ContainsKey(Vector2.up + position),
                _takenPositionsMap.ContainsKey(Vector2.down + position),
                _takenPositionsMap.ContainsKey(Vector2.left + position),
                _takenPositionsMap.ContainsKey(Vector2.right + position));
            room.SetType(position == Vector2.zero ? GameManager.RoomType.Starting : GameManager.RoomType.Normal);
            if (bossRoom == null) bossRoom = room;
            if (CheckDistance(bossRoomPos, position))
            {
                bossRoom = room;
                bossRoomPos = position;
            }
        }

        if (bossRoom != null) bossRoom.SetType(GameManager.RoomType.Boss);
    }

    private bool CheckDistance(Vector2 vec1, Vector2 vec2)
    {
        var dist1 = Mathf.Abs(vec1.x) + Mathf.Abs(vec1.y);
        var dist2 = Mathf.Abs(vec2.x) + Mathf.Abs(vec2.y);
        return (dist1 < dist2);
    }
}
