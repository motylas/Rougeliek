using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    // ReSharper disable Unity.PerformanceAnalysis
    public virtual void PrimarySpell()
    {
        if (!Input.GetKeyDown(KeyCode.Mouse0)) return;
        Debug.Log("NOT YET IMPLEMENTED");
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public virtual void SecondarySpell()
    {
        if (!Input.GetKeyDown(KeyCode.Mouse1)) return;
        Debug.Log("NOT YET IMPLEMENTED");
    }
}