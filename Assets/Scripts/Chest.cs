using System;
using System.Collections.Generic;
using UnityEngine;


public class Chest : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    [SerializeField] private Sprite chestOpen;
    private bool isOpen = false;
    [SerializeField] private List<PowerUps> powerUpsList;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!isOpen && GameManager.Instance.keys > 0)
        {
            GameManager.Instance.keys--;
            _spriteRenderer.sprite = chestOpen;
            isOpen = true;
            Instantiate(powerUpsList[MyUtils.Random.Next(powerUpsList.Count)], transform.position, Quaternion.identity, transform);
        }
    }
}