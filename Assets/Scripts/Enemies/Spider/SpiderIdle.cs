using UnityEngine;


public class SpiderIdle : EnemyState
{
    private Spider _spider;
    private float _idleTime;
    private float _stateDuration;
    [SerializeField] private float minDurationTime;
    [SerializeField] private float maxDurationTime;

    [SerializeField] private float chanceToMoveTowardsPlayer;
    
    private void Start()
    {
        _spider = GetComponent<Spider>();
        stateName = "Spider_Idle";
    }

    public override void OnEnterState()
    {
        _idleTime = Time.time;
        _stateDuration = MyUtils.RandomFloat(minDurationTime, maxDurationTime);
    }

    public override EnemyState OnUpdateState()
    {
        if (Time.time - _idleTime <= _stateDuration) return this;
        return MyUtils.RandomFloat(0f, 1f) < chanceToMoveTowardsPlayer ? _spider.movePlayerState : _spider.moveRandomState;
    }

    public override void OnLeaveState()
    {
        
    }
}