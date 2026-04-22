using Fusion;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : NetworkBehaviour
{
    public enum State
    {
        Patrol,
        Chase
    }

    [Header("Patrol")]
    private Transform[] patrolPoints;
    private int currentPoint = 0;

    [Header("Wait At Point")]
    public float waitTime = 3f;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    [Header("Detect (FOV)")]
    public float viewDistance = 10f;
    public float viewAngle = 60f;
    public LayerMask obstacleMask;

    [Header("Chase")]
    public float chaseSpeed = 5f;
    public float patrolSpeed = 2f;

    [Header("Attack")]
    public float attackRange = 2f;

    private NavMeshAgent agent;
    private Animator anim;
    private Transform player;

    [Networked] private State currentState { get; set; }

    public override void Spawned()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        // ?? FIX 1: ép enemy xu?ng NavMesh (tránh bay)
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
        }

        // ?? FIX 2: l?y patrol points t? manager
        if (PatrolPointManager.Instance != null)
        {
            patrolPoints = PatrolPointManager.Instance.points;
        }

        // ?? tìm player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        if (Object.HasStateAuthority)
        {
            currentState = State.Patrol;
            GoToNextPoint();
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        if (agent == null || !agent.isOnNavMesh) return;

        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                DetectPlayer();
                break;

            case State.Chase:
                Chase();
                break;
        }
    }

    // ================= PATROL =================
    void Patrol()
    {
        agent.speed = patrolSpeed;

        // ?ang ch?
        if (isWaiting)
        {
            waitTimer -= Runner.DeltaTime;

            anim.SetBool("IsRun", false);
            anim.SetBool("IsAttack", false);

            if (waitTimer <= 0f)
            {
                isWaiting = false;
                GoToNextPoint();
            }

            return;
        }

        // ?ang ch?y
        anim.SetBool("IsRun", true);
        anim.SetBool("IsAttack", false);

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            isWaiting = true;
            waitTimer = waitTime;

            agent.SetDestination(transform.position);
        }
    }

    void GoToNextPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        agent.SetDestination(patrolPoints[currentPoint].position);
        currentPoint = (currentPoint + 1) % patrolPoints.Length;
    }

    // ================= DETECT =================
    void DetectPlayer()
    {
        if (player == null) return;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= viewDistance)
        {
            float angle = Vector3.Angle(transform.forward, dirToPlayer);

            if (angle <= viewAngle / 2f)
            {
                if (!Physics.Raycast(transform.position + Vector3.up, dirToPlayer, distance, obstacleMask))
                {
                    currentState = State.Chase;
                    isWaiting = false;
                }
            }
        }
    }

    // ================= CHASE =================
    void Chase()
    {
        if (player == null)
        {
            currentState = State.Patrol;
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            agent.SetDestination(transform.position);
            anim.SetBool("IsRun", false);
            anim.SetBool("IsAttack", true);

            transform.LookAt(player);
        }
        else
        {
            agent.speed = chaseSpeed;
            agent.SetDestination(player.position);

            anim.SetBool("IsRun", true);
            anim.SetBool("IsAttack", false);
        }

        if (distance > viewDistance * 1.5f)
        {
            currentState = State.Patrol;
            isWaiting = false;
            GoToNextPoint();
        }
    }

    // ================= DIE =================
    public void Die()
    {
        if (!Object.HasStateAuthority) return;

        anim.SetTrigger("Die");
        agent.isStopped = true;
    }

    // ================= DEBUG =================
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 left = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;

        Gizmos.DrawRay(transform.position, left * viewDistance);
        Gizmos.DrawRay(transform.position, right * viewDistance);
        Gizmos.DrawRay(transform.position, transform.forward * viewDistance);
    }
}