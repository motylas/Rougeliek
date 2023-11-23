using UnityEngine;

// Player Controller for other scripts
public class Player : MonoBehaviour
{
    // Player Components
    private PlayerFight _playerFight;
    private PlayerMovement _playerMovement;
    public Transform fireTransform;
    
    
    void Start()
    {
        // Get attached components
        _playerFight = gameObject.GetComponent<PlayerFight>();
        _playerMovement = gameObject.GetComponent<PlayerMovement>();
    }

    public void ReceiveDamage(int damage)
    {
        _playerFight.ReceiveDamage(damage);
    }
}