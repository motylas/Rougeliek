using System;
using System.Collections.Generic;
using UnityEngine;


public class Chest : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    [SerializeField] private Sprite chestOpen;
    private bool isOpen = false;
    [SerializeField] private List<PowerUp> powerUpsList;
    [SerializeField] private List<Ability> abilitiesList;

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
            GameManager.Instance.keysTextMeshPro.text = GameManager.Instance.keys.ToString();
            _spriteRenderer.sprite = chestOpen;
            isOpen = true;
            if (0.5 > MyUtils.Random.NextDouble())
                Instantiate(powerUpsList[MyUtils.Random.Next(powerUpsList.Count)], transform.position, Quaternion.identity, transform);
            else Instantiate(abilitiesList[MyUtils.Random.Next(abilitiesList.Count)], transform.position, Quaternion.identity, transform);
        }
    }
}