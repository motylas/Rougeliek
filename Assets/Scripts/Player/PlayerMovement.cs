using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Movement variables that can be modified
    [SerializeField] private float force = 200f;
    
    // Storing input
    private float _horizontal;
    private float _vertical;
    
    // Other movement variables
    [SerializeField]private Rigidbody2D rb;
    private Vector2 _moveDirection = Vector2.zero;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    private void Update()
    {
        // Get input from player
        _horizontal = Input.GetAxisRaw("Horizontal");
        _vertical = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        var horizontal = _horizontal;
        var vertical = _vertical;

        _moveDirection = new Vector2(horizontal, vertical).normalized;
        rb.AddRelativeForce(force*_moveDirection, ForceMode2D.Impulse);
    }
}
