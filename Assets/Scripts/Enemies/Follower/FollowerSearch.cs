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
    public class FollowerSearch : EnemyState
    {
        private Follower _follower;
        private Transform _player;
        public Vector2 playerLastSeen;

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
            _player = GameManager.Instance.player.transform;
            _rb = GetComponent<Rigidbody2D>();
            stateName = "follower_search";
            
            colliderTransform = circleCollider2D.transform;
        }

        public override void OnEnterState()
        {
            _pathFind = StartCoroutine(FindPathToPlayer());
        }

        public override EnemyState OnUpdateState()
        {
            var followerPosition = (Vector2) colliderTransform.position;
            if (Vector2.Distance(_player.position, followerPosition) < _follower.chaseArea) return _follower.followingState;
            
            if (followPath.Count < 1) return _follower.patrollingState;
            if (Vector2.Distance(playerLastSeen, followerPosition) < 0.25f) return _follower.patrollingState;
            
            if (followerPosition == followPath[0]) followPath.RemoveAt(0);
            if (followPath.Count == 0) return this;
            _moveDirection = (followPath[0] - followerPosition).normalized;
            _rb.AddRelativeForce(force * _moveDirection, ForceMode2D.Impulse);
            return this;
        }
        
        public override void OnLeaveState()
        {
            StopCoroutine(_pathFind);
        }
        
        private IEnumerator FindPathToPlayer()
        {
            while (true)
            {
                var newPath = _follower.FindPath(playerLastSeen);
                if (newPath != null) followPath = newPath;
                yield return new WaitForSeconds(0.1f);
            }
            // ReSharper disable once IteratorNeverReturns
        }
    }
}