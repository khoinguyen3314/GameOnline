using Fusion;
using UnityEngine;

public class DamageableFusion : NetworkBehaviour
{
    [Header("Health")]
    [Networked] public int CurrentHP { get; set; }
    [SerializeField] public int MaxHP = 10;
    [SerializeField] NetworkBehaviour die;
    [Networked] private TickTimer DeathTimer { get; set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            CurrentHP = MaxHP;
        }
    }

    public void InflictDamage(int damage, GameObject source)
    {
        if (!Object.HasStateAuthority) return;

        // Nếu đã chết rồi thì khỏi trừ nữa
        if (DeathTimer.IsRunning) return;

        CurrentHP -= damage;

        if (CurrentHP <= 0)
        {
            StartDeathCountdown();
        }
    }

    private void StartDeathCountdown()
    {
        die.enabled = false;
        // Set timer 3 giây
        DeathTimer = TickTimer.CreateFromSeconds(Runner, 3f);
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        if (DeathTimer.Expired(Runner))
        {
            Die();
        }
    }

    private void Die()
    {
        if (Runner != null)
        {
            Runner.Despawn(Object);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}