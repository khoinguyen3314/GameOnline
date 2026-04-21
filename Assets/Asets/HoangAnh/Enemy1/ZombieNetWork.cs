using Fusion;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class ZombieAI_Network : NetworkBehaviour
{
    [Header("Components")]
    private NavMeshAgent agent;
    private Animator anim;

    [Header("AI Settings")]
    public float chaseRange = 10f;    // Ph?m vi phát hi?n player
    public float attackRange = 1.5f; // Ph?m vi ?? t?n công
    public float patrolRadius = 5f;  // Ph?m vi ?i tu?n

    [Networked] public int health { get; set; } = 100;

    private Transform _target;
    private float _lastAttackTime;
    private Vector3 _patrolPoint;

    public override void Spawned()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        // Thi?t l?p t?c ?? di chuy?n ban ??u
        agent.speed = 2f;

        if (Object.HasStateAuthority)
        {
            SetNewPatrolDestination();
        }
    }

    public override void FixedUpdateNetwork()
    {
        // QUAN TR?NG: Ch? Host/Server m?i ch?y logic tính toán AI
        if (!Object.HasStateAuthority) return;

        FindNearestPlayer();

        if (_target != null)
        {
            float distance = Vector3.Distance(transform.position, _target.position);

            if (distance <= attackRange)
            {
                AttackLogic(distance);
            }
            else if (distance <= chaseRange)
            {
                ChaseLogic();
            }
            else
            {
                PatrolLogic();
            }
        }
        else
        {
            PatrolLogic();
        }
    }

    private void FindNearestPlayer()
    {
        float closestDistance = chaseRange;
        _target = null;

        // Quét tìm t?t c? các Player ?ang online trong Runner
        foreach (var playerRef in Runner.ActivePlayers)
        {
            NetworkObject playerObj = Runner.GetPlayerObject(playerRef);
            if (playerObj == null) continue;

            float dist = Vector3.Distance(transform.position, playerObj.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                _target = playerObj.transform;
            }
        }
    }

    private void ChaseLogic()
    {
        agent.isStopped = false;
        agent.speed = 3.5f; // Chuy?n sang t?c ?? ch?y
        agent.SetDestination(_target.position);

        anim.SetBool("Walk", false);
        anim.SetBool("Run", true); // Set Run khi ?u?i theo
    }

    private void AttackLogic(float distance)
    {
        agent.isStopped = true;
        anim.SetBool("Run", false);
        anim.SetBool("Walk", false);

        // Quay m?t v? phía Player khi ?ánh
        Vector3 lookPos = _target.position - transform.position;
        lookPos.y = 0;
        if (lookPos != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPos), Runner.DeltaTime * 10f);
        }

        if (Time.time > _lastAttackTime + 1.5f)
        {
            anim.SetTrigger("ATK"); // Kích ho?t ATK
            _lastAttackTime = Time.time;
        }

        if (distance > attackRange)
        {
            ChaseLogic();
        }
    }

    private void PatrolLogic()
    {
        agent.isStopped = false;
        agent.speed = 1.5f; // T?c ?? ?i b? tu?n tra

        anim.SetBool("Run", false);
        anim.SetBool("Walk", true); // Set Walk khi ?i tu?n

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            SetNewPatrolDestination();
        }
    }

    private void SetNewPatrolDestination()
    {
        Vector3 randomDir = Random.insideUnitSphere * patrolRadius;
        randomDir += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDir, out hit, patrolRadius, 1))
        {
            agent.SetDestination(hit.position);
        }
    }

    // RPC ?? nh?n sát th??ng t? b?t k? Player nào chém trúng
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            anim.SetTrigger("Die");
            agent.isStopped = true;
            // Ng?ng script AI sau khi ch?t
            this.enabled = false;
        }
    }
}