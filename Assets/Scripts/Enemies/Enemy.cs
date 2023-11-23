using UnityEngine;

public abstract class Enemy: MonoBehaviour
{
    private EnemyState CurrentState;
    private Animator Animator;

    [SerializeField] private int healthPoints;
    private Player _player;
    

    private void ChangeState(EnemyState newState)
    {
        if (newState.stateName == CurrentState.stateName) return;
        CurrentState.OnLeaveState();
        Animator.Play(newState.stateName);
        newState.OnEnterState();
        CurrentState = newState;
    }

    protected void Setup(EnemyState startingState)
    {
        _player = GameManager.Instance.player;
        Animator = GetComponent<Animator>();
        CurrentState = startingState;
        startingState.OnEnterState();
    }
    
    private void FixedUpdate()
    {
        ChangeState(CurrentState.OnUpdateState());
    }
    
    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player")) _player.ReceiveDamage(1);
    }
    
    public void GetDamage(int damage)
    {
        healthPoints -= damage;
        if (healthPoints <= 0) Die();
    }
    
    private void Die()
    {
        Destroy(gameObject);
    }
}