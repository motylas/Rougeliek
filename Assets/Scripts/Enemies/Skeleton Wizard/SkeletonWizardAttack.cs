using Unity.VisualScripting;
using UnityEngine;


public class SkeletonWizardAttack : EnemyState
{
    [SerializeField] private GameObject bulletPrefab;
    private bool _hasAnimated;
    private SkeletonWizard _skeletonWizard;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float force;

    private void Start()
    {
        stateName = "sw_attack";
        _skeletonWizard = GetComponent<SkeletonWizard>();
    }

    public override void OnEnterState()
    {
        _hasAnimated = false;
    }

    public override EnemyState OnUpdateState()
    {
        return _hasAnimated ? _skeletonWizard.moveState : this;
    }
    
    public override void OnLeaveState()
    {
        
    }
    
    public void OnAttack()
    {
        var lookDir = GameManager.Instance.player.transform.position - transform.position;
        // var angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        var bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);//Quaternion.AngleAxis(angle, Vector3.forward));
        bullet.GetComponent<Rigidbody2D>().AddForce(force * lookDir.normalized , ForceMode2D.Impulse);
    }
    
    public void HasAnimated()
    {
        _hasAnimated = true;
    }
}