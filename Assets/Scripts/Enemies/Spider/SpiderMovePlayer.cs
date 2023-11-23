using UnityEngine;


public class SpiderMovePlayer : EnemyState
{
    private Spider _spider;
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
    
    
    
    private void Start()
    {
        _spider = GetComponent<Spider>();
        stateName = "Spider_Move_Player";
        _rb = GetComponent<Rigidbody2D>();
        _player = GameManager.Instance.player;
    }

    public override void OnEnterState()
    {
        _moveTime = Time.time;
        _stateDuration = MyUtils.RandomFloat(minDurationTime, maxDurationTime);
        _moveDirection = (_player.transform.position - transform.position).normalized; // do this to get local position of player
    }

    public override EnemyState OnUpdateState()
    {
        if (Time.time - _moveTime > _stateDuration) return _spider.idleState;
        var force = MyUtils.RandomFloat(minForce,maxForce);
        _rb.AddRelativeForce(force * _moveDirection, ForceMode2D.Impulse);
        return this;
    }

    public override void OnLeaveState()
    {
        
    }
}
