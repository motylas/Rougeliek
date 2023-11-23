using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;


public class SkeletonWizardMove : EnemyState
{
    private SkeletonWizard _skeletonWizard;
    
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
    
    [SerializeField] private float escapeRange;
    private float _currentRange;

    private SpriteRenderer _spriteRenderer;
    [SerializeField] private Transform firePoint;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        stateName = "sw_move";
        _skeletonWizard = GetComponent<SkeletonWizard>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
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
        _moveDirection = new Vector2(_xAxisDirection, _yAxisDirection).normalized;
        OnFlip(_moveDirection.x > 0f);
    }
    
    private void OnFlip(bool flip)
    {
        if (_spriteRenderer.flipX != flip) firePoint.localPosition *= new Vector2(-1f, 1);
        _spriteRenderer.flipX = flip;
    }

    public override EnemyState OnUpdateState()
    {
        if (Time.time - _moveTime > _stateDuration) return _skeletonWizard.attackState;
        _currentRange = Vector2.Distance(transform.position, GameManager.Instance.player.transform.position);
        if (_currentRange < escapeRange) return _skeletonWizard.escapeState;
        var force = MyUtils.RandomFloat(minForce,maxForce);
        _rb.AddForce(force * _moveDirection, ForceMode2D.Impulse);
        return this;
    }
    
    public override void OnLeaveState()
    {
        
    }
}