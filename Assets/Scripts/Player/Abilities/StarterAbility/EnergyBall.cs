using System;
using UnityEngine;

public class EnergyBall : MonoBehaviour
{
    [SerializeField] private int damage;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        // _spriteRenderer = GetComponent<SpriteRenderer>();
        // GameManager.Instance.projectilesList.Add(_spriteRenderer);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Enemy")) return;
        other.gameObject.GetComponent<Enemy>().GetDamage((int) (damage * GameManager.Instance.player.GetDamageMultiplayer()));
        GameManager.Instance.projectilesList.Remove(_spriteRenderer);
        Destroy(gameObject);
    }
}