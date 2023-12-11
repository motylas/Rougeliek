using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public bool canBeTaken = true;
    public virtual void OnStart(){}
    
    // ReSharper disable Unity.PerformanceAnalysis
    public virtual void UseAbility()
    {
        if (!Input.GetKeyDown(KeyCode.Mouse0)) return;
        Debug.Log("NOT YET IMPLEMENTED");
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!Input.GetKey(KeyCode.E)) return;
        if(canBeTaken)
            GameManager.Instance.player.SetCurrentAbility(this);
    }
}