using Fusion;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAIFusion : NetworkBehaviour
{
    [Header("Range")]
    public float detectRange = 10f;
    public float attackRange = 2f;

    [Header("Attack")]
    public int damage = 10;
    public float attackCooldown = 1.2f;
    private TickTimer attackTimer;

    [Header("Components")]
    public NavMeshAgent agent;
    public NetworkMecanimAnimator netAnim;

    private Transform target;
    private PlayerHealthFusion targetHealth;

    private bool isReady = false; // 🔥 tránh chạy sớm gây teleport

    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;

        StartCoroutine(InitAgent());
    }

    IEnumerator InitAgent()
    {
        // 🔥 đợi 1 frame để đảm bảo position sync xong
        yield return null;

        if (agent != null)
        {
            agent.enabled = false; // tắt trước khi warp
            agent.Warp(transform.position); // 🔥 FIX TELEPORT
            agent.enabled = true;

            agent.stoppingDistance = Mathf.Max(attackRange - 0.1f, 0.1f);
            agent.isStopped = true;
        }

        isReady = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        if (!isReady) return; // 🔥 chưa init xong thì không chạy

        // Nếu target chết hoặc null → tìm lại
        if (target == null || targetHealth == null || !targetHealth.IsAlive())
        {
            FindTarget();
        }

        if (target == null)
        {
            GoIdle();
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
            Chase();
        }
        else
        {
            Attack();
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

        agent.isStopped = false;

        // 🔥 chỉ set destination khi hợp lệ
        if (NavMesh.SamplePosition(target.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        if (netAnim != null)
        {
            netAnim.Animator.SetBool("IsRun", true);
            netAnim.Animator.SetBool("IsAttack", false);
        }
    }

    // ================= ATTACK =================

    void Attack()
    {
        if (target == null || targetHealth == null)
            return;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        Vector3 lookPos = target.position;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);

        if (netAnim != null)
        {
            netAnim.Animator.SetBool("IsRun", false);
            netAnim.Animator.SetBool("IsAttack", true);
        }

        if (attackTimer.ExpiredOrNotRunning(Runner))
        {
            attackTimer = TickTimer.CreateFromSeconds(Runner, attackCooldown);

            if (netAnim != null)
                netAnim.SetTrigger("Attack");

            DealDamage();
        }
    }

    void DealDamage()
    {
        if (targetHealth == null) return;
        if (!targetHealth.IsAlive()) return;

        targetHealth.RPC_RequestTakeDamage(damage);
    }

    // ================= IDLE =================

    void GoIdle()
    {
        if (netAnim != null)
        {
            netAnim.Animator.SetBool("IsRun", false);
            netAnim.Animator.SetBool("IsAttack", false);
        }

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
    }

    void ClearTarget()
    {
        target = null;
        targetHealth = null;
        GoIdle();
    }
}