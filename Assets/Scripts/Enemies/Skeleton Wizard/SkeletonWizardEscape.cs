using UnityEngine;


public class SkeletonWizardEscape : EnemyState
{
    private SkeletonWizard _skeletonWizard;
    private float _moveTime;
    private float _stateDuration;
    [SerializeField] private float minDurationTime;
    [SerializeField] private float maxDurationTime;
    
    // Moving
    private Rigidbody2D _rb;
    private Player _player;
    [SerializeField] private int minForce;
    [SerializeField] private int maxForce;
    private Vector2 _moveDirection;

    [SerializeField] private float safeRange;
    private float _currentRange;
    
    private SpriteRenderer _spriteRenderer;
    [SerializeField] private Transform firePoint;
    
    public void Start()
    {
        _skeletonWizard = GetComponent<SkeletonWizard>();
        stateName = "sw_escape";
        _rb = GetComponent<Rigidbody2D>();
        _player = GameManager.Instance.player;
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    public override void OnEnterState()
    {
        _moveTime = Time.time;
        _stateDuration = MyUtils.RandomFloat(minDurationTime, maxDurationTime);
        _moveDirection = -(_player.transform.position - transform.position).normalized; // do this to get local position of player
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
        if (_currentRange > safeRange) return _skeletonWizard.attackState;
        var force = MyUtils.RandomFloat(minForce,maxForce);
        _rb.AddRelativeForce(force * _moveDirection, ForceMode2D.Impulse);
        return this;
    }

    public override void OnLeaveState()
    {
        
    }
}
