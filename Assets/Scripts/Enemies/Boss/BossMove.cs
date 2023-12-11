using System;
using Unity.VisualScripting;
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
            return;
        }


        var noiseX = MyUtils.RandomFloat(0f, .4f) - 0.2f;
        var noiseY = MyUtils.RandomFloat(0f, .4f) - 0.2f;
        _moveDirection = GameManager.Instance.player.transform.position - _boss.circleCollider2D.transform.position;
        _moveDirection.x += noiseX;
        _moveDirection.x += noiseY;
        _moveDirection.Normalize();
    }
    
    
}