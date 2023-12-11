using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO ADD VISION TO ENEMIES
// TODO REMAKE SKELETON
// TODO REPAIR MASS AND FORCE
public class PlayerVision : MonoBehaviour
{
    [SerializeField] private float visionArea;
    [SerializeField] private Transform wallCollider;

    // Load on room enter
    private Room _currentRoom;

    // Colors to put on tiles
    private Color _basicColor;
    private Color _seenColor;
    private Color _blackColor;
    private Color _transparentColor;
    private Color _seenTransparencyColor;

    private List<SpriteRenderer> _rocksList;
    private List<SpriteRenderer> _boxesList;
    private List<SpriteRenderer> _bushesList;
    private List<SpriteRenderer> _floorList;
    private List<SpriteRenderer> _wallsList;
    private readonly List<RaycastHit2D> _raycastHit2Ds = new();
    [SerializeField] private ContactFilter2D contactFilter2D;

    private int _layerMask;

    private void Start()
    {
        _basicColor = Color.white;
        _seenColor = new Color(0.2f, 0.2f, 0.2f, 1);
        _blackColor = Color.black;
        _transparentColor = new Color(0f, 0f, 0f, 0);
        _seenTransparencyColor = new Color(0f, 0f, 0f, 0.8f);
    }


    public void OnRoomEnter()
    {
        SetCurrentRoom();
        StartCoroutine(RayTracing());
    }


    private void SetCurrentRoom()
    {
        _currentRoom = GameManager.Instance.currentRoom;
        _rocksList = _currentRoom.rocksList;
        _boxesList = _currentRoom.boxesList;
        _bushesList = _currentRoom.bushesList;
        _floorList = _currentRoom.floorsList;
        _wallsList = _currentRoom.wallsList;

        // SET COLOR
        
        SetColor(_rocksList, _blackColor);
        SetColor(_boxesList, _blackColor);
        SetColor(_bushesList, _blackColor);
        SetColor(_floorList, _blackColor);
        SetColor(_wallsList, _blackColor);
    }

    private IEnumerator RayTracing()
    {
        while (true)
        {
            SetColorDistance(_rocksList, _seenColor);
            SetColorDistance(_boxesList, _seenColor);
            SetColorDistance(_bushesList, _seenColor);
            SetColorDistance(_floorList, _seenColor);
            SetColorDistance(_wallsList, _seenTransparencyColor);
            HideEnemies();
            HideProjectiles();
            var raysToCast = 360;
            var angle = 0f;
            var angleIncrease = 360f / raysToCast;
            for (var i = 0; i < raysToCast; i++)
            {
                var angleRad = angle * (Mathf.PI / 180f);
                var direction = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
                _raycastHit2Ds.Clear();
                Physics2D.Raycast(wallCollider.position, direction, contactFilter2D, _raycastHit2Ds,
                    visionArea);
                var showFloor = false;
                foreach (var hit in _raycastHit2Ds)
                {
                    if (hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("Projectile"))
                        hit.collider.gameObject.GetComponent<SpriteRenderer>().enabled = true;
                    else if (hit.collider.CompareTag("WallV"))
                    {
                        hit.collider.gameObject.GetComponent<SpriteRenderer>().color = _transparentColor;
                    }
                    else
                    {
                        
                        hit.collider.gameObject.GetComponent<SpriteRenderer>().color = _basicColor;
                        if (showFloor) break;
                        if (hit.collider.CompareTag("RockV")) showFloor = true;
                        if (hit.collider.CompareTag("BushV"))
                        {
                            if (Vector2.Distance(hit.collider.transform.position, wallCollider.position) > 0.7f)
                                showFloor = true;
                        }
                    }
                }
                angle -= angleIncrease;
            }

            yield return new WaitForSeconds(0.02f);
        }
    }

    private void HideEnemies()
    {
        foreach (var enemy in GameManager.Instance.enemiesList)
        {
            enemy.GetComponentInParent<SpriteRenderer>().enabled = false;
        }
    }   
    private void HideProjectiles()
    {
        foreach (var sprite in GameManager.Instance.projectilesList)
        {
            if (sprite) sprite.enabled = false;
        }
    }


    public void OnRoomClear()
    {
        StopAllCoroutines();
        SetColor(_floorList, _basicColor);
        SetColor(_rocksList, _basicColor);
        SetColor(_boxesList, _basicColor);
        SetColor(_bushesList, _basicColor);
        SetColor(_wallsList, _transparentColor);
    }

    private void SetColor(List<SpriteRenderer> obstaclesList, Color color)
    {
        foreach (var sprite in obstaclesList)
        {
            sprite.color = color;
        }
    }
    
    private void SetColorDistance(List<SpriteRenderer> obstaclesList, Color color)
    {
        foreach (var sprite in obstaclesList)
        {
            if (sprite.color == _basicColor) sprite.color = color;
            if (sprite.color == _transparentColor) sprite.color = color;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(wallCollider.position, visionArea); // Chase area draw
    }
}