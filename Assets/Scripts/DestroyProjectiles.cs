using UnityEngine;

public class DestroyProjectiles : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Projectile")) Destroy(other.gameObject);
    }
}
