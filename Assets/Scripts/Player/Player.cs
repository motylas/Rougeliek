using System;
using System.Globalization;
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

    private Camera _camera;
    
    void Start()
    {
        // Get attached components
        _playerFight = gameObject.GetComponent<PlayerFight>();
        _playerMovement = gameObject.GetComponent<PlayerMovement>();
        _playerVision = gameObject.GetComponent<PlayerVision>();
        _camera = Camera.main;
    }

    private void Update()
    {
        var transform1 = _camera.transform;
        var transform2 = transform;
        transform1.position = new Vector3(transform2.position.x, transform2.position.y, transform1.position.z);
    }

    public void ReceiveDamage(int damage)
    {
        _playerFight.ReceiveDamage(damage);
    }
    
    public void HealthUp(int life)
    {
        _playerFight.hp += life;
        GameManager.Instance.healthTextMeshPro.text = _playerFight.hp.ToString();
    }
    
    public void StrengthUp(float strength)
    {
        _playerFight.strength *= strength;
        GameManager.Instance.strengthTextMeshPro.text = _playerFight.strength.ToString(CultureInfo.InvariantCulture);
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