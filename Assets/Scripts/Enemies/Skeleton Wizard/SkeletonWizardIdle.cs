using UnityEngine;


public class SkeletonWizardIdle : EnemyState
{
    private EnemyState _moveState;
    private float _stateDuration;
    private float _idleTime;
    
    private void Start()
    {
        stateName = "sw_idle";
        _moveState = GetComponent<SkeletonWizardMove>();
    }

    public override void OnEnterState()
    {
        _idleTime = Time.time;
        _stateDuration = 0.5f;
    }

    public override EnemyState OnUpdateState()
    {
        return Time.time - _idleTime <= _stateDuration ? this : _moveState;
    }
    
    public override void OnLeaveState()
    {
        
    }
}