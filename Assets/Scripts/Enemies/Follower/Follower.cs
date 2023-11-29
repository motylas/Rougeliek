using System;
using UnityEngine;

namespace Follower
{
    public class Follower : Enemy
    {
        [HideInInspector] public EnemyState followingState;

        private void Start()
        {
            followingState = GetComponent<FollowerFollowing>();
            
            Setup(followingState);
        }
    }
}