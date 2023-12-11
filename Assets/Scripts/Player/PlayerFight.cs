using System.Collections;
using System.Globalization;
using UnityEngine;

public class PlayerFight : MonoBehaviour
{
    // Variables used when player receives damage that can be modified
    public int hp = 6;
    public float strength = 1;
    [SerializeField] private float playersImmortalityTime = 1f;
    
    // Variables used when damage is received
    private LiveState _liveState;
    private SpriteRenderer _playerSpriteRenderer;
    private Coroutine _takeDamageCoroutine;
    
    // Current Ability
    [SerializeField] public Ability ability;
    [SerializeField] private Ability startingAbility;

    private void Start()
    {
        ability = Instantiate(startingAbility, transform.position, Quaternion.identity);
        ability.spriteRenderer.enabled = false;
        ability.canBeTaken = false;
        GameManager.Instance.currentWeaponImage.sprite = ability.spriteRenderer.sprite;
        GameManager.Instance.currentWeaponImage.color = ability.spriteRenderer.color;
        ability.OnStart();
        ReceiveDamageSetup();
        GameManager.Instance.strengthTextMeshPro.text = strength.ToString(CultureInfo.InvariantCulture);
        GameManager.Instance.healthTextMeshPro.text = hp.ToString();
    }
    
    private void Update()
    {
        UseAbility();
    }
    
    // Setup Methods //
    
    private void ReceiveDamageSetup()
    {
        _liveState = LiveState.Normal;
        _playerSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Attack Methods //
    

    private void UseAbility()
    {
        ability.UseAbility();
    }
    
    // Receive Damage Methods //

    public void ReceiveDamage(int damage)
    {
        if (_liveState == LiveState.Immortal) return;
        StartCoroutine(Immortality());
        hp -= damage;
        if (hp <= 0) Die();
    }

    private void Die()
    {
        Debug.Log("DEAD");
        Destroy(gameObject);
    }

    private IEnumerator Immortality()
    {
        _takeDamageCoroutine = StartCoroutine(Blinking());
        _liveState = LiveState.Immortal;
        yield return new WaitForSeconds(playersImmortalityTime);
        _liveState = LiveState.Normal;
        StopCoroutine(_takeDamageCoroutine);
        _playerSpriteRenderer.enabled = true;
    }

    private IEnumerator Blinking()
    {
        while (true)
        {
            _playerSpriteRenderer.enabled = !_playerSpriteRenderer.enabled;
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    // States when Player takes damage
    private enum LiveState
    {
        Immortal,
        Normal
    }
}