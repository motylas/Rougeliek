using System;
using UnityEngine;

namespace Follower
{
    public class Follower : Enemy
    {
        [HideInInspector] public EnemyState followingState;
        [HideInInspector] public EnemyState patrollingState;
        [HideInInspector] public EnemyState searchingState;

        private void Start()
        {
            followingState = GetComponent<FollowerFollowing>();
            patrollingState = GetComponent<FollowerPatrol>();
            searchingState = GetComponent<FollowerSearch>();
            
            Setup(patrollingState);
        }
    }
}