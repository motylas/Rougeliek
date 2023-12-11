using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FollowerPatrol : EnemyState
{
    private Follower _follower;
    private Transform _player;

    // Movement
    [SerializeField] private int force;
    private Vector2 _moveDirection;
    private Rigidbody2D _rb;
    private int layerMask;
    private Vector3 randomPosition;


    // Path Find
    private List<Vector2> followPath = new();
    private Coroutine _pathFind;
    private Transform colliderTransform;
    private bool playerSeen;
    
    // For seeing the player


    private void Start()
    {
        _follower = GetComponent<Follower>();
        _player = GameManager.Instance.player.transform;
        _rb = GetComponent<Rigidbody2D>();
        stateName = "follower_patrolling";

        colliderTransform = _follower.circleCollider2D.transform;
        layerMask = LayerMask.GetMask("RockV") | LayerMask.GetMask("BushV") | LayerMask.GetMask("Player");
    }

    public override void OnEnterState()
    {
        _follower = GetComponent<Follower>();
        randomPosition = MyUtils.GetRandomPositionInRoom();
        _pathFind = StartCoroutine(FindPathToPlayer());
        StartCoroutine(PlayerSeen());
    }

    public override EnemyState OnUpdateState()
    {
        var followerPosition = (Vector2)colliderTransform.position;
        if (Vector2.Distance(_player.position, followerPosition) < _follower.chaseArea)
        {
            if (playerSeen)
                return _follower.followingState;
        }

        if (followPath.Count < 1 || Vector2.Distance(randomPosition, followerPosition) < 0.25f)
        {
            randomPosition = MyUtils.GetRandomPositionInRoom();
            return this;
        }

        if (followerPosition == followPath[0]) followPath.RemoveAt(0);
        if (followPath.Count == 0) return this;

        _moveDirection = (followPath[0] - followerPosition).normalized;
        _rb.AddRelativeForce(force * _moveDirection, ForceMode2D.Impulse);
        return this;
    }

    public override void OnLeaveState()
    {
        StopAllCoroutines();
    }

    private IEnumerator FindPathToPlayer()
    {
        while (true)
        {
            var newPath = _follower.FindPath(randomPosition);
            if (newPath != null) followPath = newPath;
            yield return new WaitForSeconds(0.1f);
        }
        // ReSharper disable once IteratorNeverReturns
    }

    private IEnumerator PlayerSeen()
    {
        playerSeen = false;
        var raysToCast = 360;
        var angle = 0f;
        var angleIncrease = 360f / raysToCast;
        for (var i = 0; i < raysToCast; i++)
        {
            var angleRad = angle * (Mathf.PI / 180f);
            var direction = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
            var hit = Physics2D.Raycast(_follower.circleCollider2D.transform.position, direction, _follower.chaseArea,
                layerMask);
            if (!hit)
            {
                Debug.DrawLine(_follower.circleCollider2D.transform.position, _follower.circleCollider2D.transform.position+direction*_follower.chaseArea, Color.blue);
                continue;
            }
            Debug.DrawLine(_follower.circleCollider2D.transform.position, hit.point, Color.blue);
            if (hit.collider.CompareTag("Player")) playerSeen = true;
            angle -= angleIncrease;
        }
        yield return new WaitForSeconds(0.02f);
    }
}