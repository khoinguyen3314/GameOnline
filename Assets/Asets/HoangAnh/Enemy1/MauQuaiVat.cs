using Fusion;
using UnityEngine;

public class MauQuaiVat : NetworkBehaviour
{
    [Networked] public int HP { get; set; } = 10;

    public override void Spawned()
    {
        // Chỉ máy có quyền (Host/StateAuthority) set máu ban đầu
        if (Object.HasStateAuthority)
        {
            HP = 10;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Chỉ máy có quyền mới xử lý damage
        if (!Object.HasStateAuthority) return;

        if (other.CompareTag("HitBox"))
        {
            HP -= 5;

            if (HP <= 0)
            {
                Runner.Despawn(Object);
            }
        }
    }
}