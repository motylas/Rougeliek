using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;


public class FollowerFollowing : EnemyState
{
    private Follower _follower;
    private Player _player;

    // Movement
    [SerializeField] private int force;
    private Vector2 _moveDirection;
    private Rigidbody2D _rb;
    private Transform colliderTransform;

    // Path Find
    [SerializeField] private CircleCollider2D circleCollider2D;
    private List<Vector2> followPath = new();
    private Coroutine _pathFind;
    

    private void Start()
    {
        _follower = GetComponent<Follower>();
        _player = GameManager.Instance.player;
        _rb = GetComponent<Rigidbody2D>();
        stateName = "follower_following";

        colliderTransform = circleCollider2D.transform;
    }

    public override void OnEnterState()
    {
        StartCoroutine(FindPathToPlayer());
    }

    public override EnemyState OnUpdateState()
    {
        var playerPosition = (Vector2)_player.transform.position;
        var followerPosition = (Vector2)colliderTransform.position;
        if (Vector2.Distance(playerPosition, followerPosition) > _follower.chaseArea || !_follower.PlayerSeen()) return _follower.searchingState;
        
        if (followPath.Count == 0) return this;
        if (followerPosition == followPath[0]) followPath.RemoveAt(0);
        if (followPath.Count == 0) return this;
        _moveDirection = (followPath[0] - followerPosition).normalized;
        _rb.AddRelativeForce(force * _moveDirection, ForceMode2D.Impulse);
        return this;
    }

    public override void OnLeaveState()
    {
        StopAllCoroutines();
        var state = (FollowerSearch)_follower.searchingState;
        state.playerLastSeen = _player.wallCollider.position;
    }
    
    private IEnumerator FindPathToPlayer()
    {
        while (true)
        {
            var newPath = _follower.FindPath(GameManager.Instance.player.wallCollider.position);
            if (newPath != null) followPath = newPath;
            yield return new WaitForSeconds(0.1f);
        }
        // ReSharper disable once IteratorNeverReturns
    }
}