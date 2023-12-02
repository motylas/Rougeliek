using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;

public static class MyUtils
{
    public static readonly Random Random = new();
    private const float RadiusOffset = 0.02f;
    private static readonly List<RaycastHit2D> CastResults = new();

    public static float RandomFloat(float min, float max)
    {
        return (float)(Random.NextDouble() * (max - min) + min);
    }

    public static List<Vector2> FindPath(Vector3 startPosition, Vector3 finalPosition, CircleCollider2D circleCollider2D, ContactFilter2D contactFilter2D)
    {
        var currentRoom = GameManager.Instance.currentRoom;
        var tilemap = currentRoom.rocksTilemap;
        
        var firstCellInRoom = tilemap.WorldToCell(currentRoom.firstCellTransform.position);
        var startingCell = tilemap.WorldToCell(startPosition);
        var finalCell = tilemap.WorldToCell(finalPosition);
        
        List<Vector3Int> visitedNodes = new() { startingCell };
        var id = 0;
        
        Cell currentCell = null;
        var nodesList = new SortedDictionary<CellID, Cell> { { new CellID(0f, id++), new Cell(startingCell, null, 0) }};
        
        while (true)
        {
            if (nodesList.Count == 0) return currentCell != null ? MakePath(currentCell, tilemap, circleCollider2D, contactFilter2D) : null;
            currentCell = nodesList.First().Value;
            
            // If destination found, make new path and reset values
            if (currentCell.position == finalCell) return MakePath(currentCell, tilemap, circleCollider2D, contactFilter2D);
            
            // Get rest values
            var currentDistance = nodesList.First().Key;
            nodesList.Remove(currentDistance);
            
            // Calculate relative tile position
            var horizontalGrid = currentCell.position.x - firstCellInRoom.x;
            var verticalGrid = firstCellInRoom.y - currentCell.position.y;
                
            // Fix calculations
            if (verticalGrid < 0) verticalGrid = 0;
            if (verticalGrid >= currentRoom.gridHeight) verticalGrid = currentRoom.gridHeight-1;
            if (horizontalGrid < 0) horizontalGrid = 0;
            if (horizontalGrid >= currentRoom.gridWidth) horizontalGrid = currentRoom.gridWidth-1;
                
            // Check adjacent tiles
            var up = new Vector3Int(currentCell.position.x, currentCell.position.y+1);
            var left = new Vector3Int(currentCell.position.x-1, currentCell.position.y);
            var right = new Vector3Int(currentCell.position.x+1, currentCell.position.y);
            var down = new Vector3Int(currentCell.position.x, currentCell.position.y-1);
        
        
            if (horizontalGrid > 0 && !tilemap.HasTile(left) && !visitedNodes.Contains(left))
            {
                nodesList.Add(
                    new CellID(currentCell.rawDistance + 1f + Vector3.Distance(finalPosition, tilemap.CellToWorld(left)), id++),
                    new Cell(left,currentCell,currentCell.rawDistance + 1f));
                visitedNodes.Add(left);
            }
                
            if (horizontalGrid < currentRoom.gridWidth-1 && !tilemap.HasTile(right) && !visitedNodes.Contains(right))
            {
                nodesList.Add(
                    new CellID(currentCell.rawDistance + 1f + Vector3.Distance(finalPosition, tilemap.CellToWorld(right)), id++),
                    new Cell(right,currentCell,currentCell.rawDistance + 1f));
                visitedNodes.Add(right);
            }
                
            if (verticalGrid > 0 && !tilemap.HasTile(up) && !visitedNodes.Contains(up))
            {
                nodesList.Add(
                    new CellID(currentCell.rawDistance + 1f + Vector3.Distance(finalPosition, tilemap.CellToWorld(up)), id++), 
                    new Cell(up,currentCell,currentCell.rawDistance + 1f));
                visitedNodes.Add(up);
            }
                
            // ReSharper disable once InvertIf
            if (verticalGrid < currentRoom.gridHeight-1 && !tilemap.HasTile(down) && !visitedNodes.Contains(down))
            {
                nodesList.Add(
                    new CellID(currentCell.rawDistance + 1f + Vector3.Distance(finalPosition, tilemap.CellToWorld(down)), id++),
                    new Cell(down,currentCell,currentCell.rawDistance + 1f));
                visitedNodes.Add(down);
            }
        }
    }
    
    private static List<Vector2> MakePath(Cell currentCell, Tilemap tileMap, CircleCollider2D circleCollider2D, ContactFilter2D contactFilter2D)
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
            Debug.DrawLine(finalPath[i-1], finalPath[i],Color.red, .05f);
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