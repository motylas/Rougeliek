using UnityEngine;

// Player Controller for other scripts
public class Player : MonoBehaviour
{
    // Player Components
    private PlayerFight _playerFight;
    private PlayerMovement _playerMovement;
    private PlayerVision _playerVision;
    public Transform fireTransform;
    public Transform wallCollider;
    
    void Start()
    {
        // Get attached components
        _playerFight = gameObject.GetComponent<PlayerFight>();
        _playerMovement = gameObject.GetComponent<PlayerMovement>();
        _playerVision = gameObject.GetComponent<PlayerVision>();
    }

    public void ReceiveDamage(int damage)
    {
        _playerFight.ReceiveDamage(damage);
    }

    public void OnStartVision()
    {
        _playerVision.OnRoomEnter();
    }
    
    public void OnEndVision()
    {
        _playerVision.OnRoomClear();
    }
}