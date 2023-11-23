using System;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    private bool _hitWall;
    
    private void Start()
    {
        _hitWall = true;
        // Invoke(nameof(SetHitWall), .02f);
        Invoke(nameof(Destroy), 5f);
    }
    
    private void SetHitWall()
    {
        _hitWall = false;
    }
    
    private void Destroy()
    {
        Destroy(gameObject);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Untagged")) return;
        if (_hitWall && other.gameObject.CompareTag("Wall")) return;
        if (other.gameObject.CompareTag("Player")) return;
        if (other.isTrigger) return;
        if (other.gameObject.CompareTag("Enemy")) other.gameObject.GetComponent<Enemy>().GetDamage(5);
        Destroy(gameObject);
    }
}