using System;
using UnityEngine;


public class StrengthUp : PowerUps
{
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!Input.GetKey(KeyCode.E)) return;
        GameManager.Instance.player.StrengthUp(1.5f);
        Destroy(gameObject);
    }
}