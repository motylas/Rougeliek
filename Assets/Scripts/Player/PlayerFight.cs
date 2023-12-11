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
    private Ability _ability;

    private void Start()
    {
        _ability = gameObject.AddComponent<StarterAbility>();
        ReceiveDamageSetup();
        GameManager.Instance.strengthTextMeshPro.text = strength.ToString(CultureInfo.InvariantCulture);
        GameManager.Instance.healthTextMeshPro.text = hp.ToString();
    }
    
    private void Update()
    {
        Spells();
    }
    
    // Setup Methods //
    
    private void ReceiveDamageSetup()
    {
        _liveState = LiveState.Normal;
        _playerSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Attack Methods //
    
    private void Spells()
    {
        PrimarySpell();
        SecondarySpell();
    }

    private void PrimarySpell()
    {
        _ability.PrimarySpell();
    }
    
    private void SecondarySpell()
    {
        _ability.SecondarySpell();
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