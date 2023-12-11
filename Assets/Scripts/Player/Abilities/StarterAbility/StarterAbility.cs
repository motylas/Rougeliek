using System;
using UnityEngine;
using UnityEngine.Serialization;

public class StarterAbility : Ability
{
    [SerializeField] private float force = 60;
    private Camera _mainCamera;
    private Rigidbody2D _playerRb;
    [SerializeField] private GameObject spell;
    private Player _player;

    private void Start()
    {
        OnStart();
    }

    public override void OnStart()
    {
        _mainCamera = Camera.main;
        _player = GameManager.Instance.player;
        _playerRb = _player.gameObject.GetComponent<Rigidbody2D>();
    }

    public override void UseAbility()
    {
        if (!Input.GetKeyDown(KeyCode.Mouse0)) return;
        Vector2 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        var lookDir = mousePos - _playerRb.position;
        var angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        var bullet = Instantiate(spell, _player.fireTransform.position, Quaternion.AngleAxis(angle, Vector3.forward));
        bullet.GetComponent<Rigidbody2D>().AddForce(force * lookDir.normalized , ForceMode2D.Impulse);
    }
}