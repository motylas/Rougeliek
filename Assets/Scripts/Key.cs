using TMPro;
using UnityEngine;


public class Key : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        GameManager.Instance.keys++;
        GameManager.Instance.keysTextMeshPro.text = GameManager.Instance.keys.ToString();
        Destroy(gameObject);
    }
}