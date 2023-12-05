using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace Follower
{
    public class FollowerFollowing : EnemyState
    {
        private Follower _follower;
        private Player _player;

        // Movement
        [SerializeField] private int force;
        private Vector2 _moveDirection;
        private Rigidbody2D _rb;
        private Transform colliderTransform;
        private float colliderRadius;
        private int layerMask; 

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
            colliderRadius = circleCollider2D.radius;
            layerMask = LayerMask.GetMask("WallBodyColl") | LayerMask.GetMask("Player");
        }

        public override void OnEnterState()
        {
            _pathFind = StartCoroutine(FindPathToPlayer());
        }

        public override EnemyState OnUpdateState()
        {
            var playerPosition = (Vector2) _player.transform.position;
            var followerPosition = (Vector2) colliderTransform.position;
            if (Vector2.Distance(playerPosition, followerPosition) > _follower.chaseArea) return _follower.searchingState;
            
            _moveDirection = (playerPosition - followerPosition).normalized;
            var rayHit = Physics2D.CircleCast(followerPosition, colliderRadius, _moveDirection, Mathf.Infinity, layerMask);
            if (rayHit.collider.CompareTag("Player"))
            {
                _rb.AddRelativeForce(force * _moveDirection, ForceMode2D.Impulse);
                return this;
            }
            if (followPath.Count == 0) return this;
            if (followerPosition == followPath[0]) followPath.RemoveAt(0);
            if (followPath.Count == 0) return this;
            _moveDirection = (followPath[0] - followerPosition).normalized;
            _rb.AddRelativeForce(force * _moveDirection, ForceMode2D.Impulse);
            return this;
        }
        
        public override void OnLeaveState()
        {
            StopCoroutine(_pathFind);
            var state = (FollowerSearch)_follower.searchingState;
            state.playerLastSeen = _player.wallCollider.position;
        }

        private IEnumerator FindPathToPlayer()
        {
            while (true)
            {
                var newPath = _follower.FindPath(_player.wallCollider.position);
                if (newPath != null) followPath = newPath;
                yield return new WaitForSeconds(0.1f);
            }
            // ReSharper disable once IteratorNeverReturns
        }
    }
}