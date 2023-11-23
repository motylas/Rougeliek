using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonWizard : Enemy
{
    [HideInInspector] public EnemyState idleState; 
    [HideInInspector] public EnemyState attackState; 
    [HideInInspector] public EnemyState moveState; 
    [HideInInspector] public EnemyState escapeState; 
    
    private void Start()
    {
        idleState = GetComponent<SkeletonWizardIdle>();
        attackState = GetComponent<SkeletonWizardAttack>();
        moveState = GetComponent<SkeletonWizardMove>();
        escapeState = GetComponent<SkeletonWizardEscape>();
        
        Setup(idleState);
    }
}
