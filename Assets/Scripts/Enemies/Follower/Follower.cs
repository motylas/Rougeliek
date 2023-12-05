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
            colliderTransform = circleCollider2D.transform;
            
            Setup(patrollingState);
        }

        private void Update()
        {
            if (CurrentState.stateName == followingState.stateName) color = Color.red;
            if (CurrentState.stateName == searchingState.stateName) color = Color.blue;
            if (CurrentState.stateName == patrollingState.stateName) color = Color.green;
        }
    }
}