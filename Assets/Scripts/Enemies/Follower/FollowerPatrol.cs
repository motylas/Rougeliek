using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Follower
{
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
        
        

        private void Start()
        {
            _follower = GetComponent<Follower>();
            _player = GameManager.Instance.player.transform;
            _rb = GetComponent<Rigidbody2D>();
            stateName = "follower_patrolling";
            
            colliderTransform = _follower.circleCollider2D.transform;
        }

        public override void OnEnterState()
        {
            _follower = GetComponent<Follower>();
            randomPosition = MyUtils.GetRandomPositionInRoom();
            _pathFind = StartCoroutine(FindPathToPlayer());
        }

        public override EnemyState OnUpdateState()
        {
            var followerPosition = (Vector2) colliderTransform.position;
            if (Vector2.Distance(_player.position, followerPosition) < _follower.chaseArea) 
                return _follower.followingState;

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
            StopCoroutine(_pathFind);
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

        
    }
}