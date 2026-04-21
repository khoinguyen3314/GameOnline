using Fusion;
using UnityEngine;
using UnityEngine.AI;

// 1. Khai báo các trạng thái của Enemy
public enum EnemyState
{
    Idle,
    Chase,
    Attack
}

public class EnemyRangeFusion : NetworkBehaviour
{
    // 2. Biến Networked đồng bộ trạng thái tới mọi máy
    [Header("State")]
    [Networked, OnChangedRender(nameof(OnStateChanged))]
    public EnemyState CurrentState { get; set; }

    [Header("Target")]
    private Transform target;
    private PlayerHealthFusion targetHealth;

    [Header("Range")]
    public float detectRange = 20f;
    public float attackRange = 15f;

    [Header("Attack")]
    public float attackCooldown = 2f;
    private TickTimer attackTimer;

    [Header("Bullet")]
    public NetworkPrefabRef bulletPrefab;
    public Transform firePoint;

    [Header("Components")]
    public NavMeshAgent agent;
    public NetworkMecanimAnimator netAnim;

    public override void Spawned()
    {
        if (agent != null)
        {
            agent.stoppingDistance = attackRange - 0.5f;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
            return;

        if (target == null || targetHealth == null || !targetHealth.IsAlive())
        {
            FindTarget();
        }

        if (target == null)
        {
            CurrentState = EnemyState.Idle; // Gán state
            Idle();
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > detectRange)
        {
            ClearTarget();
            return;
        }

        if (distance > attackRange)
        {
            CurrentState = EnemyState.Chase; // Gán state
            Chase();
        }
        else
        {
            CurrentState = EnemyState.Attack; // Gán state
            Attack();
        }
    }

    // ================= ANIMATION SYNC =================

    // 3. Hàm này tự động chạy trên mọi máy khi CurrentState thay đổi
    void OnStateChanged()
    {
        if (netAnim != null && netAnim.Animator != null)
        {
            switch (CurrentState)
            {
                case EnemyState.Idle:
                    netAnim.Animator.SetBool("IsRun", false);
                    netAnim.Animator.SetBool("IsAttack", false);
                    break;

                case EnemyState.Chase:
                    netAnim.Animator.SetBool("IsRun", true);
                    netAnim.Animator.SetBool("IsAttack", false);
                    break;

                case EnemyState.Attack:
                    netAnim.Animator.SetBool("IsRun", false);
                    netAnim.Animator.SetBool("IsAttack", true);
                    break;
            }
        }
    }

    // ================= TARGET =================

    void FindTarget()
    {
        float closestDistance = Mathf.Infinity;
        PlayerHealthFusion closest = null;

        foreach (var player in PlayerManager.Players)
        {
            if (player == null) continue;
            if (!player.IsAlive()) continue;

            float dist = Vector3.Distance(transform.position, player.transform.position);

            if (dist < closestDistance && dist <= detectRange)
            {
                closestDistance = dist;
                closest = player;
            }
        }

        if (closest != null)
        {
            target = closest.transform;
            targetHealth = closest;
        }
        else
        {
            ClearTarget();
        }
    }

    // ================= CHASE =================

    void Chase()
    {
        if (agent == null || !agent.isOnNavMesh || target == null)
            return;

        agent.updateRotation = true;
        agent.isStopped = false;
        agent.SetDestination(target.position);
    }

    // ================= ATTACK =================

    void Attack()
    {
        if (target == null) return;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.updateRotation = false;
        }

        // 🎯 xoay mặt chuẩn (NetworkTransform sẽ lo việc đồng bộ xoay này cho client)
        Vector3 lookPos = target.position;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);

        if (attackTimer.ExpiredOrNotRunning(Runner))
        {
            attackTimer = TickTimer.CreateFromSeconds(Runner, attackCooldown);

            // NetworkMecanimAnimator tự động đồng bộ Trigger rất tốt từ Server xuống Client
            if (netAnim != null)
                netAnim.SetTrigger("Attack");

            FireBullet();
        }
    }

    //================= FIRE =================

    void FireBullet()
    {
        if (bulletPrefab == null || firePoint == null || target == null) return;

        // 🎯 Aim vào thân (không bắn xuống chân)
        Vector3 targetPos = target.position + Vector3.up * 1.2f;
        Vector3 direction = (targetPos - firePoint.position).normalized;

        Quaternion rot = Quaternion.LookRotation(direction);

        NetworkObject bulletObj = Runner.Spawn(
            bulletPrefab,
            firePoint.position,
            rot,
            Object.StateAuthority
        );

        var bullet = bulletObj.GetComponent<BulletFusion>();
        if (bullet != null)
        {
            bullet.Init(direction);
        }
    }

    // ================= IDLE =================

    void Idle()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }
    }

    void ClearTarget()
    {
        target = null;
        targetHealth = null;
        CurrentState = EnemyState.Idle; // Đồng bộ về Idle khi mất mục tiêu
        Idle();
    }
}