using UnityEngine;

public abstract class EnemyState : MonoBehaviour
{
    [HideInInspector] public string stateName;

    // ReSharper disable Unity.PerformanceAnalysis
    public virtual void OnEnterState()
    {
        Debug.LogWarning("NOT YET IMPLEMENTED");
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    public virtual EnemyState OnUpdateState()
    {
        Debug.LogWarning("NOT YET IMPLEMENTED");
        return null;
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    public virtual void OnLeaveState()
    {
        Debug.LogWarning("NOT YET IMPLEMENTED");
    }
}
