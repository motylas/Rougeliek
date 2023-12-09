using System;
using Unity.VisualScripting;
using UnityEngine;


public class SkeletonWizardBullet : MonoBehaviour
{
    private bool _hitWall;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.enabled = false;
        GameManager.Instance.projectilesList.Add(_spriteRenderer);
        _hitWall = true;
        Invoke(nameof(SetHitWall), .02f);
        Invoke(nameof(Destroy), 3f);
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
        if (other.gameObject.CompareTag("Enemy")) return;
        if (other.isTrigger) return;
        if (other.gameObject.CompareTag("Player")) other.gameObject.GetComponent<Player>().ReceiveDamage(1);
        GameManager.Instance.projectilesList.Remove(_spriteRenderer);
        Destroy(gameObject);
    }
}