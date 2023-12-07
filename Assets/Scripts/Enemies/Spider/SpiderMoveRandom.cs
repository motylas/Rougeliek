using System.Collections.Generic;
using UnityEngine;


public class SpiderMoveRandom : EnemyState
{
    private Spider _spider;
    private float _moveTime;
    private float _stateDuration;
    [SerializeField] private float minDurationTime;
    [SerializeField] private float maxDurationTime;
    
    // Moving
    private Rigidbody2D _rb;
    [SerializeField] private int minForce;
    [SerializeField] private int maxForce;
    private Vector2 _moveDirection;
    private float _xAxisDirection;
    private float _yAxisDirection;
    private readonly List<RaycastHit2D> _castCollisions = new();
    [SerializeField] private ContactFilter2D contactFilter;
    [SerializeField] private float collisionOffset;
    
    
    
    private void Start()
    {
        _spider = GetComponent<Spider>();
        stateName = "Spider_Move_Random";
        _rb = GetComponent<Rigidbody2D>();
    }

    public override void OnEnterState()
    {
        _moveTime = Time.time;
        _stateDuration = MyUtils.RandomFloat(minDurationTime, maxDurationTime);
        _xAxisDirection = MyUtils.RandomFloat(-1f,1f);
        _yAxisDirection = MyUtils.RandomFloat(-1f,1f);
        var count = _rb.Cast(new Vector2(_xAxisDirection,0), contactFilter, _castCollisions,
            Time.fixedDeltaTime + collisionOffset);
        if (count > 0) _xAxisDirection = -_xAxisDirection;
        count = _rb.Cast(new Vector2(0,_yAxisDirection), contactFilter, _castCollisions,
            Time.fixedDeltaTime + collisionOffset);
        if (count > 0) _yAxisDirection = -_yAxisDirection;
        
    }

    public override EnemyState OnUpdateState()
    {
        if (Time.time - _moveTime > _stateDuration) return _spider.idleState;
        var force = MyUtils.RandomFloat(minForce,maxForce);
        _rb.AddForce(force * _moveDirection, ForceMode2D.Impulse);
        return this;
    }

    public override void OnLeaveState()
    {
        
    }
}
