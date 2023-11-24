using System;
using UnityEngine;

namespace Follower
{
    public class FollowerFollowing : EnemyState
    {
        private Follower _follower;

        private void Start()
        {
            _follower = GetComponent<Follower>();
            
        }

        public override void OnEnterState()
        {
            
        }

        public override EnemyState OnUpdateState()
        {
            return this;
        }

        public override void OnLeaveState()
        {
        }
    }
}