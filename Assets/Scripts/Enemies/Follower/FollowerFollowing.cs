using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Follower
{
    public class FollowerFollowing : EnemyState
    {
        private Follower _follower;
        private Player _player;

        private Vector2 _moveDirection;
        private Rigidbody2D _rb;
        [SerializeField] private int force;

        [SerializeField] private CircleCollider2D circleCollider2D;

        private Room _room;
        private Tilemap _tilemap;
        private SortedDictionary<CellID, Cell> nodesList;

        private List<Vector2> followPath = new();
        private readonly List<RaycastHit2D> castResults = new();
        [SerializeField] private ContactFilter2D contactFilter2D;
        [SerializeField] private float radiusOffset;
        private Coroutine _pathFind;
        [SerializeField] private float chaseArea;
        
        private void Start()
        {

            _follower = GetComponent<Follower>();
            _player = GameManager.Instance.player;
            _rb = GetComponent<Rigidbody2D>();
            stateName = "follower_following";
            Invoke(nameof(Delay), 1f); //TODO move this
        }

        private void Delay()
        {
            _room = GameManager.Instance.currentRoom;
            _tilemap = _room.rocksTilemap;
        }

        public override void OnEnterState()
        {
            _pathFind = StartCoroutine(FindPathToPlayer());
        }

        public override EnemyState OnUpdateState()
        {
            if (Vector2.Distance(_player.transform.position, circleCollider2D.transform.position) > chaseArea) return _follower.searchingState;
            _moveDirection = (_player.transform.position - circleCollider2D.transform.position).normalized;
            var rayHit = Physics2D.CircleCast(circleCollider2D.transform.position, circleCollider2D.radius, _moveDirection, Mathf.Infinity, LayerMask.GetMask("WallBodyColl")|LayerMask.GetMask("Player"));
            if (rayHit.collider.CompareTag("Player"))
            {
                _rb.AddRelativeForce(force * _moveDirection, ForceMode2D.Impulse);
                return this;
            }
            if (followPath.Count == 0) return this;
            var currentPos = (Vector2)circleCollider2D.transform.position;
            if (currentPos == followPath[0]) followPath.RemoveAt(0);
            if (followPath.Count == 0) return this;
            _moveDirection = (followPath[0] - (Vector2) circleCollider2D.transform.position).normalized;
            _rb.AddRelativeForce(force * _moveDirection, ForceMode2D.Impulse);
            Debug.DrawLine(circleCollider2D.transform.position, rayHit.point, Color.magenta, 0f, true);
            return this;
        }

        private IEnumerator FindPathToPlayer()
        {
            while (true)
            {
                followPath = MyUtils.FindPath(circleCollider2D.transform.position, _player.wallCollider.position,
                    circleCollider2D, contactFilter2D);
                yield return new WaitForSeconds(0.02f);
            }
        }

        private IEnumerator FindPathToPlayer2()
        {
            // yield return new WaitForSeconds(1.1f); //TODO delete after
            
            // Init
            var playerPos = _player.wallCollider.position;
            var startingCell = _tilemap.WorldToCell(circleCollider2D.transform.position);
            var finalCell = _tilemap.WorldToCell(playerPos);
            var firstCellTransform = GameManager.Instance.currentRoom.firstCellTransform;
            var firstCell = _tilemap.WorldToCell(firstCellTransform.position);
            Cell currentCell = null;
            List<Vector3Int> visitedNodes = new() { startingCell };
            var id = 0;
            
            nodesList = new SortedDictionary<CellID, Cell> { { new CellID(0f, id++), new Cell(startingCell, null, 0) } };
            while (true)
            {
                if (nodesList.Count == 0)
                {
                    if (currentCell != null) MakePath(currentCell);
                    visitedNodes = new List<Vector3Int>();
                    nodesList = new SortedDictionary<CellID, Cell>();
                    playerPos = _player.wallCollider.position;
                    startingCell = _tilemap.WorldToCell(circleCollider2D.transform.position);
                    finalCell = _tilemap.WorldToCell(playerPos);
                    id = 0;
                    
            
                    nodesList.Add(new CellID(0f, id++), new Cell(startingCell, null, 0));
                    yield return new WaitForSeconds(.05f);
                    continue;
                }
                
                currentCell = nodesList.First().Value;
                var currentDistance = nodesList.First().Key;
                nodesList.Remove(currentDistance);
                
                // If destination found, make new path and reset values
                if (currentCell.position == finalCell)
                {
                    MakePath(currentCell);
                    visitedNodes = new List<Vector3Int>();
                    nodesList = new SortedDictionary<CellID, Cell>();
                    playerPos = _player.wallCollider.position;
                    startingCell = _tilemap.WorldToCell(circleCollider2D.transform.position);
                    finalCell = _tilemap.WorldToCell(playerPos);
                    id = 0;
                    
            
                    nodesList.Add(new CellID(0f, id++), new Cell(startingCell, null, 0));
                    yield return new WaitForSeconds(.05f);
                    continue;
                }
                
                // Calculate relative tile position
                
                var horizontalGrid = currentCell.position.x - firstCell.x;
                var verticalGrid = firstCell.y - currentCell.position.y;
                
                // Fix calculations
                
                if (verticalGrid < 0) verticalGrid = 0;
                if (verticalGrid >= _room.gridHeight) verticalGrid = _room.gridHeight-1;
                if (horizontalGrid < 0) horizontalGrid = 0;
                if (horizontalGrid >= _room.gridWidth) horizontalGrid = _room.gridWidth-1;
                
                // Check adjacent tiles
                
                var up = new Vector3Int(currentCell.position.x, currentCell.position.y+1);
                var left = new Vector3Int(currentCell.position.x-1, currentCell.position.y);
                var right = new Vector3Int(currentCell.position.x+1, currentCell.position.y);
                var down = new Vector3Int(currentCell.position.x, currentCell.position.y-1);
        
        
                if (horizontalGrid > 0 && !_tilemap.HasTile(left) && !visitedNodes.Contains(left))
                {
                    nodesList.Add(
                        new CellID(currentCell.rawDistance + 1f + Vector3.Distance(playerPos, _tilemap.CellToWorld(left)), id++),
                        new Cell(left,currentCell,currentCell.rawDistance + 1f));
                    visitedNodes.Add(left);
                }
                
                if (horizontalGrid < _room.gridWidth-1 && !_tilemap.HasTile(right) && !visitedNodes.Contains(right))
                {
                        nodesList.Add(
                            new CellID(currentCell.rawDistance + 1f + Vector3.Distance(playerPos, _tilemap.CellToWorld(right)), id++),
                            new Cell(right,currentCell,currentCell.rawDistance + 1f));
                        visitedNodes.Add(right);
                }
                
                if (verticalGrid > 0 && !_tilemap.HasTile(up) && !visitedNodes.Contains(up))
                {
                        nodesList.Add(
                            new CellID(currentCell.rawDistance + 1f + Vector3.Distance(playerPos, _tilemap.CellToWorld(up)), id++), 
                            new Cell(up,currentCell,currentCell.rawDistance + 1f));
                        visitedNodes.Add(up);
                }
                
                // ReSharper disable once InvertIf
                if (verticalGrid < _room.gridHeight-1 && !_tilemap.HasTile(down) && !visitedNodes.Contains(down))
                {
                    nodesList.Add(
                        new CellID(currentCell.rawDistance + 1f + Vector3.Distance(playerPos, _tilemap.CellToWorld(down)), id++),
                        new Cell(down,currentCell,currentCell.rawDistance + 1f));
                    visitedNodes.Add(down);
                }
            }
        }
        
        private void MakePath(Cell currentCell)
        {
            List<Vector2> path = new();
            var lastPos = _tilemap.CellToWorld(currentCell.position);
            lastPos.x += .5f;
            lastPos.y += .5f;
            path.Add(lastPos);
            if (currentCell.lastNode == null) return;
            currentCell = currentCell.lastNode;
            var currentPos = _tilemap.CellToWorld(currentCell.position);
            currentPos.x += .5f;
            currentPos.y += .5f;
            path.Add(currentPos);
            while (true)
            {
                if (currentCell.lastNode == null) break;
                currentCell = currentCell.lastNode;
                currentPos = _tilemap.CellToWorld(currentCell.position);
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
                if (Physics2D.CircleCast(path[0], circleCollider2D.radius+radiusOffset, (path[id] - path[0]), contactFilter2D, castResults, Vector2.Distance(path[id], path[0])) != 0) continue;
                if (id == path.Count-1) break;
                
                var betweenVec = path[id];
                var previousVec = path[id];
                while (true)
                {
                    betweenVec = Vector2.Lerp(betweenVec, path[id+1], 0.05f);
                    if (Physics2D.CircleCast(path[0], circleCollider2D.radius+radiusOffset, (betweenVec - path[0]), contactFilter2D, castResults,
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
            followPath = finalPath;
        }
        
        

        public override void OnLeaveState()
        {
            StopCoroutine(_pathFind);
            var state = (FollowerSearch)_follower.searchingState;
            state.playerLastSeen = _player.wallCollider.position;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, chaseArea);
            // Gizmos.DrawWireCube(box.transform.position + (Vector3)_moveDirection * collisionOffset, box.size);
            // Gizmos.DrawCube(transform.position + (Vector3)box.offset + (Vector3)_moveDirection * collisionOffset, box.size);
        }
    }
    
    internal class Cell
    {
        public Vector3Int position;
        public Cell lastNode;
        public float rawDistance;
    
        public Cell(Vector3Int position, Cell node, float d)
        {
            this.position = position;
            lastNode = node;
            rawDistance = d;
        }
    }
    
    internal class CellID : IComparable<CellID>
    {
        public float distance;
        public int ID;

        public CellID(float distance, int id)
        {
            this.distance = distance;
            ID = id;
        }

        public int CompareTo(CellID other)
        {
            var distanceComparison = distance.CompareTo(other.distance);

            return distanceComparison == 0 ? ID.CompareTo(other.ID) : distanceComparison;
        }
    }
}