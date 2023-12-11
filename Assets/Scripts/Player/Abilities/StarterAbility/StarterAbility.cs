using UnityEngine;

public class StarterAbility : Ability
{
    [SerializeField] private float force = 60;
    private Camera _mainCamera;
    private Rigidbody2D _playerRb;
    private GameObject _primarySpell;
    private GameObject _secondarySpell;
    private Player _player;
    
    private void Start()
    {
        _mainCamera = Camera.main;
        _player = GameManager.Instance.player;
        _playerRb = _player.gameObject.GetComponent<Rigidbody2D>();
        _primarySpell = GameManager.Instance.spellsDict[GetType().Name].primarySpell;
        // _secondarySpell = GameManager.Instance.spellsDict[GetType().Name].secondarySpell;
    }

    public override void PrimarySpell()
    {
        if (!Input.GetKeyDown(KeyCode.Mouse0)) return;
        Vector2 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        var lookDir = mousePos - _playerRb.position;
        var angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        var bullet = Instantiate(_primarySpell, _player.fireTransform.position, Quaternion.AngleAxis(angle, Vector3.forward));
        bullet.GetComponent<Rigidbody2D>().AddForce(force * lookDir.normalized , ForceMode2D.Impulse);
    }
}