using System;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    [SerializeField] private int damage;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Enemy")) return;
        other.gameObject.GetComponent<Enemy>().GetDamage(damage);
        Destroy(gameObject);
    }
}