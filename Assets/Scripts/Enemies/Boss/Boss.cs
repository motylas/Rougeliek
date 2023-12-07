using System;
using UnityEngine;


public class Boss : Enemy
{
    [HideInInspector] public EnemyState bossMove;
    [SerializeField] private Enemy spiderPrefab;
    [SerializeField] private int maxFamilyCount;
    public int maxHealth;
    public int familyCount;

    private void Start()
    {
        bossMove = GetComponent<BossMove>();
        
        Setup(bossMove);
    }

    protected override void Die()
    {
        if (maxFamilyCount != familyCount)
        {
            var spiderOne = (Boss) Instantiate(spiderPrefab, transform.position, Quaternion.identity, GameManager.Instance.currentRoom.enemiesContainer);
            spiderOne.transform.localScale *= .5f;
            var stateOne = (BossMove) spiderOne.bossMove;
            stateOne.force += 30;
            spiderOne.familyCount = familyCount + 1;
            spiderOne.healthPoints = maxHealth / 2;
            spiderOne.maxHealth = maxHealth / 2;
            spiderOne.Animator.speed *= 1.3f;
            
            var spiderTwo = (Boss) Instantiate(spiderPrefab, transform.position, Quaternion.identity, GameManager.Instance.currentRoom.enemiesContainer);
            spiderTwo.transform.localScale *= .5f;
            var stateTwo = (BossMove) spiderTwo.bossMove;
            stateTwo.force = stateOne.force;
            spiderTwo.familyCount = familyCount + 1;
            spiderTwo.healthPoints = maxHealth / 2;
            spiderTwo.maxHealth = maxHealth / 2;
            spiderTwo.Animator.speed *= 1.3f;
        }
        
        base.Die();
    }
}