using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;

public static class MyUtils
{
    public static readonly Random Random = new();
    
    // Path Find
    private const float RadiusOffset = 0.02f;
    private static readonly List<RaycastHit2D> CastResults = new();

    public static float RandomFloat(float min, float max)
    {
        return (float)(Random.NextDouble() * (max - min) + min);
    }

    public static Vector3 GetRandomPositionInRoom()
    {
        var currentRoom = GameManager.Instance.currentRoom;
        if (currentRoom == null) return Vector3.one;
        var tileMap = currentRoom.rocksTilemap;
        var maxX = (float) currentRoom.innerGridWidth;
        var maxY = (float) currentRoom.innerGridHeight;
        var firstCell = currentRoom.firstInnerCellTransform.position;
        Vector3Int cellPosition;
        do
        {
            var randomX = RandomFloat(firstCell.x, firstCell.x + maxX-1);
            var randomY = RandomFloat(firstCell.y, firstCell.y - maxY+1);
            var position = new Vector3(randomX, randomY);
            cellPosition = tileMap.WorldToCell(position);
        } while (tileMap.HasTile(cellPosition));

        var returnPosition = tileMap.CellToWorld(cellPosition);
        returnPosition.x += .5f;
        returnPosition.y += .5f;

        return returnPosition;
    }

    public static List<Vector2> FindPath(Vector3 startPosition, Vector3 finalPosition, CircleCollider2D circleCollider2D, ContactFilter2D contactFilter2D, Color color)
    {
        var currentRoom = GameManager.Instance.currentRoom;
        var rocksTilemap = currentRoom.rocksTilemap;
        var boxesTilemap = currentRoom.boxesTilemap;

        // CHECK FOR ENEMIES
        var enemiesList = GameManager.Instance.enemiesList;
        var tilesBlockedByEnemies = new List<Vector3Int>();
        foreach (var transform in enemiesList)
        {
            if (circleCollider2D.transform == transform) continue;
            tilesBlockedByEnemies.Add(rocksTilemap.WorldToCell(transform.position));
        }
        
        var firstCellInRoom = rocksTilemap.WorldToCell(currentRoom.firstInnerCellTransform.position);
        var startingCell = rocksTilemap.WorldToCell(startPosition);
        var finalCell = rocksTilemap.WorldToCell(finalPosition);
        
        List<Vector3Int> visitedNodes = new() { startingCell };
        var id = 0;

        var nodesList = new SortedDictionary<CellID, Cell> { { new CellID(0f, id++), new Cell(startingCell, null, 0) }};
        
        while (true)
        {
            if (nodesList.Count == 0) return null;
            var currentCell = nodesList.First().Value;
            
            // If destination found, make new path and reset values
            if (currentCell.position == finalCell) return MakePath(currentCell, rocksTilemap, circleCollider2D, contactFilter2D, color);
            
            // Get rest values
            var currentDistance = nodesList.First().Key;
            nodesList.Remove(currentDistance);
            
            // Calculate relative tile position
            var horizontalGrid = currentCell.position.x - firstCellInRoom.x;
            var verticalGrid = firstCellInRoom.y - currentCell.position.y;
                
            // Fix calculations
            if (verticalGrid < 0) verticalGrid = 0;
            if (verticalGrid >= currentRoom.innerGridHeight) verticalGrid = currentRoom.innerGridHeight-1;
            if (horizontalGrid < 0) horizontalGrid = 0;
            if (horizontalGrid >= currentRoom.innerGridWidth) horizontalGrid = currentRoom.innerGridWidth-1;
                
            // Check adjacent tiles
            var up = new Vector3Int(currentCell.position.x, currentCell.position.y+1);
            var left = new Vector3Int(currentCell.position.x-1, currentCell.position.y);
            var right = new Vector3Int(currentCell.position.x+1, currentCell.position.y);
            var down = new Vector3Int(currentCell.position.x, currentCell.position.y-1);
        
        
            if (horizontalGrid > 0 && !rocksTilemap.HasTile(left) && !boxesTilemap.HasTile(left) && !visitedNodes.Contains(left) && !tilesBlockedByEnemies.Contains(left))
            {
                nodesList.Add(
                    new CellID(currentCell.rawDistance + 1f + Vector3.Distance(finalPosition, rocksTilemap.CellToWorld(left)), id++),
                    new Cell(left,currentCell,currentCell.rawDistance + 1f));
                visitedNodes.Add(left);
            }
                
            if (horizontalGrid < currentRoom.innerGridWidth-1 && !rocksTilemap.HasTile(right) && !boxesTilemap.HasTile(right) && !visitedNodes.Contains(right) && !tilesBlockedByEnemies.Contains(right))
            {
                nodesList.Add(
                    new CellID(currentCell.rawDistance + 1f + Vector3.Distance(finalPosition, rocksTilemap.CellToWorld(right)), id++),
                    new Cell(right,currentCell,currentCell.rawDistance + 1f));
                visitedNodes.Add(right);
            }
                
            if (verticalGrid > 0 && !rocksTilemap.HasTile(up) && !boxesTilemap.HasTile(up) && !visitedNodes.Contains(up) && !tilesBlockedByEnemies.Contains(up))
            {
                nodesList.Add(
                    new CellID(currentCell.rawDistance + 1f + Vector3.Distance(finalPosition, rocksTilemap.CellToWorld(up)), id++), 
                    new Cell(up,currentCell,currentCell.rawDistance + 1f));
                visitedNodes.Add(up);
            }
                
            // ReSharper disable once InvertIf
            if (verticalGrid < currentRoom.innerGridHeight-1 && !rocksTilemap.HasTile(down) && !boxesTilemap.HasTile(down) && !visitedNodes.Contains(down) && !tilesBlockedByEnemies.Contains(down))
            {
                nodesList.Add(
                    new CellID(currentCell.rawDistance + 1f + Vector3.Distance(finalPosition, rocksTilemap.CellToWorld(down)), id++),
                    new Cell(down,currentCell,currentCell.rawDistance + 1f));
                visitedNodes.Add(down);
            }
        }
    }
    
    private static List<Vector2> MakePath(Cell currentCell, Tilemap tileMap, CircleCollider2D circleCollider2D, ContactFilter2D contactFilter2D, Color color)
    {
        List<Vector2> path = new();
        var lastPos = tileMap.CellToWorld(currentCell.position);
        lastPos.x += .5f;
        lastPos.y += .5f;
        path.Add(lastPos);
        if (currentCell.lastNode == null) return path;
        currentCell = currentCell.lastNode;
        var currentPos = tileMap.CellToWorld(currentCell.position);
        currentPos.x += .5f;
        currentPos.y += .5f;
        path.Add(currentPos);
        while (true)
        {
            if (currentCell.lastNode == null) break;
            currentCell = currentCell.lastNode;
            currentPos = tileMap.CellToWorld(currentCell.position);
            currentPos.x += .5f;
            currentPos.y += .5f;
            path.Add(currentPos);
        }
        path.Add(circleCollider2D.transform.position);
        path.Reverse();
            
        List<Vector2> finalPath = new();
        int id;
        finalPath.Add(path[0]); // LEAVE IT
            
        for (id = path.Count-1; id > 0; id--)
        {
            if (Physics2D.CircleCast(path[0], circleCollider2D.radius+RadiusOffset, (path[id] - path[0]), contactFilter2D, CastResults, Vector2.Distance(path[id], path[0])) != 0) continue;
            if (id == path.Count-1) break;
                
            var betweenVec = path[id];
            var previousVec = path[id];
            while (true)
            {
                betweenVec = Vector2.Lerp(betweenVec, path[id+1], 0.05f);
                if (Physics2D.CircleCast(path[0], circleCollider2D.radius+RadiusOffset, (betweenVec - path[0]), contactFilter2D, CastResults,
                        Vector2.Distance(betweenVec, path[0])) != 0)
                {
                    finalPath.Add(previousVec);
                    break;
                }
                previousVec = betweenVec;
            }
            break;
        }

        for (var i = id; i < path.Count; i++)
        {
            finalPath.Add(path[i]);
        }

        for (var i = 1; i < finalPath.Count; i++)
        {
            Debug.DrawLine(finalPath[i-1], finalPath[i],color, .1f);
        }
            
        finalPath.RemoveAt(0);
        return finalPath;
    }
}

internal class Cell
{
    public Vector3Int position;
    public readonly Cell lastNode;
    public readonly float rawDistance;
    
    public Cell(Vector3Int position, Cell node, float d)
    {
        this.position = position;
        lastNode = node;
        rawDistance = d;
    }
}
    
internal class CellID : IComparable<CellID>
{
    private readonly float distance;
    private readonly int id;

    public CellID(float distance, int id)
    {
        this.distance = distance;
        this.id = id;
    }

    public int CompareTo(CellID other)
    {
        var distanceComparison = distance.CompareTo(other.distance);

        return distanceComparison == 0 ? id.CompareTo(other.id) : distanceComparison;
    }
}