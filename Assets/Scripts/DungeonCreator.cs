using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DungeonCreator : MonoBehaviour
{
    private readonly List<Vector2> _availablePositions = new();
    private readonly List<Vector2> _takenPositions = new();
    private readonly List<Vector2> _unavailablePositions = new();
    [SerializeField] private int numberOfRooms;
    [SerializeField] private Room roomPrefab;
    private readonly Dictionary<Vector2, bool> _takenPositionsMap = new();
    

    private void Start()
    {
        GenerateRoomsPositions();
        BSP();
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

    private void BSP()
    {
        // Get the furthest room
        var bossRoomPos = Vector2.zero;
        foreach (var position in _takenPositions) if (CheckDistance(bossRoomPos, position)) bossRoomPos = position;
        var gridSize = (int) (Mathf.Abs(bossRoomPos.x) + Mathf.Abs(bossRoomPos.y))*2+1; // max distance from the furthest room doubled + 1 to make start room in centre
        
        var widths = new int[gridSize];
        var heights = new int[gridSize];
        for (var i = 0; i < gridSize; i++)
        {
            widths[i] = MyUtils.Random.Next(11,25)*2;
            heights[i] = MyUtils.Random.Next(11,25)*2;
        }
        
        foreach (var position in _takenPositions)
        {
            var roomType = GameManager.RoomType.Normal;
            if (position == Vector2.zero) roomType = GameManager.RoomType.Starting;
            if (position == bossRoomPos) roomType = GameManager.RoomType.Boss;

            var centre = (gridSize - 1) / 2;
            var width = centre + (int) position.x;
            var height = centre + (int) position.y;
            var positionX = 0f;
            var positionY = 0f;

            if (width < centre)
            {
                for (var i = centre - 1; i >= width; i--)
                {
                    positionX -= widths[i];
                }
            }
            if (width > centre)
            {
                for (var i = centre; i < width; i++)
                {
                    positionX += widths[i];
                } 
            }
            if (height < centre)
            {
                for (var i = centre; i > height; i--)
                {
                    positionY -= heights[i];
                }
            }
            if (height > centre)
            {
                for (var i = centre+1; i <= height; i++)
                {
                    positionY += heights[i];
                } 
            }

            var finalWidth = (float) widths[width];
            var finalHeight = (float) heights[height];
            
            
            var roomTemplate = new RoomTemplate(new Vector2(positionX, positionY), (int) finalWidth, (int) finalHeight, roomType,
                _takenPositionsMap.ContainsKey(Vector2.up + position),
                _takenPositionsMap.ContainsKey(Vector2.down + position),
                _takenPositionsMap.ContainsKey(Vector2.left + position),
                _takenPositionsMap.ContainsKey(Vector2.right + position));

            var finalX = positionX + (int) finalWidth / 2; // want to lose fraction
            var finalY = positionY - (int) finalHeight / 2; // want to lose fraction
            var room = Instantiate(roomPrefab, new Vector3(finalX, finalY), Quaternion.identity);
            room.CreateFromTemplate(roomTemplate);
        }
    }

    private bool CheckDistance(Vector2 vec1, Vector2 vec2)
    {
        var dist1 = Mathf.Abs(vec1.x) + Mathf.Abs(vec1.y);
        var dist2 = Mathf.Abs(vec2.x) + Mathf.Abs(vec2.y);
        return (dist1 < dist2);
    }
}

public class RoomTemplate
{
    public Vector2 position;
    public int width;
    public int height;
    public bool up;
    public bool down;
    public bool left;
    public bool right;
    public GameManager.RoomType roomType;

    public RoomTemplate(Vector2 position, int width, int height, GameManager.RoomType roomType, bool up, bool down, bool left, bool right)
    {
        this.position = position;
        this.width = width;
        this.height = height;
        this.roomType = roomType;
        this.up = up;
        this.down = down;
        this.left = left;
        this.right = right;
    }
}
