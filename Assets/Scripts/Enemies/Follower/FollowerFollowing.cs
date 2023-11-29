using System;
using System.Collections.Generic;
using UnityEngine;

namespace Follower
{
    public class FollowerFollowing : EnemyState
    {
        private Follower _follower;
        private Player _player;

        private Vector2 _moveDirection;
        private Rigidbody2D _rb;
        [SerializeField] private int force;

        private readonly List<RaycastHit2D> _castCollisions = new();
        [SerializeField] private ContactFilter2D contactFilter;
        [SerializeField] private float collisionOffset;

        private void Start()
        {
            _follower = GetComponent<Follower>();
            _player = GameManager.Instance.player;
            _rb = GetComponent<Rigidbody2D>();
            stateName = "follower_following";
        }

        public override void OnEnterState()
        {
            
        }

        public override EnemyState OnUpdateState()
        {
            _moveDirection = (_player.transform.position - transform.position).normalized;
            var count = _rb.Cast(_moveDirection, contactFilter, _castCollisions, collisionOffset);
            if (count > 0) Debug.Log("sciana");
            _rb.AddRelativeForce(force * _moveDirection, ForceMode2D.Impulse);
            return this;
        }

        public override void OnLeaveState()
        {
        }
    }
}