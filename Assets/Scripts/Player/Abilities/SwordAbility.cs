using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class SwordAbility : Ability
{
    private Camera _mainCamera;
    private Player _player;

    public float distance;
    public float radius;
    [SerializeField] ContactFilter2D contactFilter2D;
    private List<Collider2D> results;
    private Vector2 draw;
    [SerializeField] private int damage;

    private void Start()
    {
        OnStart();
    }

    public override void OnStart()
    {
        _mainCamera = Camera.main;
        _player = GameManager.Instance.player;
    }

    public override void UseAbility()
    {
        if (!Input.GetKeyDown(KeyCode.Mouse0)) return;
        Vector2 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        var playerPosition = _player.transform.position;
        var lookDir = mousePos - (Vector2) playerPosition;
        lookDir.Normalize();
        results = new List<Collider2D>();
        draw = (Vector2)playerPosition + lookDir * distance;
        Physics2D.OverlapCircle((Vector2) playerPosition + lookDir * distance, radius, contactFilter2D, results);
        foreach (var enemy in results)
        {
            if (!enemy.gameObject.CompareTag("Enemy")) return;
            enemy.gameObject.GetComponent<Enemy>().GetDamage((int) (damage * GameManager.Instance.player.GetDamageMultiplayer()));
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(draw, radius); // Chase area draw
    }
}