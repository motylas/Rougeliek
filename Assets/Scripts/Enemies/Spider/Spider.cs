

using UnityEngine;

public class Spider : Enemy
{
    [HideInInspector] public EnemyState idleState; 
    [HideInInspector] public EnemyState moveRandomState;
    [HideInInspector] public EnemyState movePlayerState;
    
    private void Start()
    {
        idleState = GetComponent<SpiderIdle>();
        moveRandomState = GetComponent<SpiderMoveRandom>();
        movePlayerState = GetComponent<SpiderMovePlayer>();

        Setup(idleState);
    }
}
