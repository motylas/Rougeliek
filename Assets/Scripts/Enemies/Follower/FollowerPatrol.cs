using System;
using UnityEngine;

namespace Follower
{
    public class FollowerPatrol : EnemyState
    {
        private Follower _follower;
        private Transform _player;
        [SerializeField] private Transform circleCollider2D;
        [SerializeField] private float chaseArea;

        private void Start()
        {
            _follower = GetComponent<Follower>();
            _player = GameManager.Instance.player.transform;
            stateName = "follower_patrolling";
        }

        public override void OnEnterState()
        {
            
        }

        public override EnemyState OnUpdateState()
        {
            if (Vector2.Distance(_player.position, circleCollider2D.position) < chaseArea)
            {
                return _follower.followingState;
            }

            return this;
        }

        public override void OnLeaveState()
        {
            
        }
    }
}