using Fusion;
using UnityEngine;

public class DamageableFusion : NetworkBehaviour
{
    [Header("Health")]
    [Networked] public int CurrentHP { get; set; }
    [SerializeField] public int MaxHP = 10;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            CurrentHP = MaxHP;
        }
    }

    public void InflictDamage(int damage, GameObject source)
    {
        // Chỉ Host xử lý damage
        if (!Object.HasStateAuthority) return;

        CurrentHP -= damage;

        if (CurrentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Xóa object toàn mạng
        if (Runner != null)
        {
            Runner.Despawn(Object);
        }
        else
        {
            Destroy(gameObject); // fallback nếu test offline
        }
    }
}