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
    
    // Key
    [SerializeField] private Key keyPrefab;
    public double keyDropRate;
    private int layerMask;
    

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
        layerMask = LayerMask.GetMask("RockV") | LayerMask.GetMask("BushV") | LayerMask.GetMask("Player");
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
        if (keyDropRate > MyUtils.Random.NextDouble())
            Instantiate(keyPrefab, colliderTransform.position, Quaternion.identity, GameManager.Instance.currentRoom.transform);
        
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
    
    public bool PlayerSeen()
    {
        var raysToCast = 360;
        var angle = 0f;
        var angleIncrease = 360f / raysToCast;
        for (var i = 0; i < raysToCast; i++)
        {
            var angleRad = angle * (Mathf.PI / 180f);
            angle -= angleIncrease;
            var direction = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
            var hit = Physics2D.Raycast(circleCollider2D.transform.position, direction, chaseArea,
                layerMask);
            if (!hit) continue;
            if (hit.collider.CompareTag("Player")) return true;
        }
        return false;
    }
}