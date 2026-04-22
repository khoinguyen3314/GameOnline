using UnityEngine;
using Fusion;

public class SoiAiI : NetworkBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private float reachDistance = 0.5f;

    [Header("Animation")]
    [SerializeField] private NetworkMecanimAnimator netAnim;

    private PatrolPointManager patrolManager;
    private int currentIndex = 0;

    public void Init(PatrolPointManager manager)
    {
        patrolManager = manager;

        if (manager != null && manager.PointCount > 0)
        {
            currentIndex = Random.Range(0, manager.PointCount);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;
        if (patrolManager == null || patrolManager.PointCount == 0)
        {
            SetIdle();
            return;
        }

        Transform target = patrolManager.GetPoint(currentIndex);
        if (target == null)
        {
            SetIdle();
            return;
        }

        Vector3 targetPos = target.position;
        targetPos.y = transform.position.y;

        Vector3 dir = (targetPos - transform.position).normalized;

        // Di chuyển
        transform.position += dir * speed * Runner.DeltaTime;

        // Quay mặt
        if (dir != Vector3.zero)
        {
            transform.forward = dir;
        }

        // 🎬 Animation RUN
        if (dir != Vector3.zero)
        {
            SetRun();
        }
        else
        {
            SetIdle();
        }

        // Đến điểm
        if (Vector3.Distance(transform.position, targetPos) < reachDistance)
        {
            currentIndex = (currentIndex + 1) % patrolManager.PointCount;
        }
    }

    // ================= ANIMATION =================

    void SetRun()
    {
        if (netAnim != null)
        {
            netAnim.Animator.SetBool("IsRun", true);
            netAnim.Animator.SetBool("IsAttack", false);
        }
    }

    void SetIdle()
    {
        if (netAnim != null)
        {
            netAnim.Animator.SetBool("IsRun", false);
            netAnim.Animator.SetBool("IsAttack", false);
        }
    }
}