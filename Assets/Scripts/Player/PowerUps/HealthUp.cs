using UnityEngine;

public class HealthUp : PowerUps
{
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!Input.GetKey(KeyCode.E)) return;
        GameManager.Instance.player.HealthUp(1);
        Destroy(gameObject);
    }
}