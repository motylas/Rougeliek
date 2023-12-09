using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy: MonoBehaviour
{
    protected EnemyState CurrentState;
    public Animator Animator;

    public int healthPoints;
    private Player _player;
    
    // Path Find
    public float chaseArea;
    public CircleCollider2D circleCollider2D;
    [SerializeField] protected ContactFilter2D contactFilter2D;
    private Transform colliderTransform;
    protected Color color = Color.green;

    private List<Vector2> globalPath = new();
    
    // Unstuck
    [SerializeField] private int force;
    private Vector2 _moveDirection;
    [SerializeField] private Rigidbody2D rb;
    
    

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
        colliderTransform = circleCollider2D.transform;
        GameManager.Instance.enemiesList.Add(colliderTransform);
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
        if (other.gameObject.CompareTag("Enemy"))
        {
            _moveDirection = - (other.transform.position - transform.position);
            rb.AddForce(_moveDirection*force, ForceMode2D.Impulse);
        }
    }
    
    public void GetDamage(int damage)
    {
        healthPoints -= damage;
        if (healthPoints <= 0) Die();
    }
    
    protected virtual void Die()
    {
        GameManager.Instance.enemiesList.Remove(colliderTransform);
        Destroy(gameObject);
    }
    
    public List<Vector2> FindPath(Vector3 destination)
    {
        var newPath = MyUtils.FindPath(colliderTransform.position, destination,
            circleCollider2D, contactFilter2D, color);
        if (newPath != null){ globalPath = newPath;}

        if (globalPath is { Count: > 0 } && Vector2.Distance(globalPath[0], colliderTransform.position) < 0.25f)
            globalPath.RemoveAt(0);
        return globalPath;
    }
        
    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, chaseArea); // Chase area draw
    }
}