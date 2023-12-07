using System;
using UnityEngine;


public class BossMove : EnemyState
{
    private Boss _boss;
    private Rigidbody2D _rb;

    // Movement
    private Vector2 _moveDirection;
    [SerializeField] public int force;

    private void Start()
    {
        _boss = GetComponent<Boss>();
        stateName = "boss_move";
        _rb = GetComponent<Rigidbody2D>();
    }


    public override void OnEnterState()
    {
        _moveDirection = new Vector2(MyUtils.RandomFloat(-1f, 1f), MyUtils.RandomFloat(-1f, 1f)).normalized;
    }

    public override EnemyState OnUpdateState()
    {
        _rb.AddForce(force * _moveDirection, ForceMode2D.Impulse);
        return this;
    }

    public override void OnLeaveState()
    {
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Enemy"))
        {
            _moveDirection = -_moveDirection;
        }

        var noise = MyUtils.RandomFloat(0f, 1f) - 0.5f;
        var collision = other.GetContact(0).collider.gameObject.name;
        if (collision.StartsWith("TopGate") || collision.StartsWith("BottomGate"))
        {
            _moveDirection.y = -_moveDirection.y + noise;
            _moveDirection.Normalize();
        }
        else if (collision.StartsWith("LeftGate") || collision.StartsWith("RightGate"))
        {
            _moveDirection.x = -_moveDirection.x + noise;
            _moveDirection.Normalize();
        }
    }
    
    
}