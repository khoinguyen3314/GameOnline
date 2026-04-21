using Fusion;
using UnityEngine;

public class NetworkDestroyTimer : NetworkBehaviour
{
    [SerializeField] float lifeTime = 1f;

    // Dùng TickTimer để đồng bộ thời gian giữa các máy
    [Networked] private TickTimer destroyTimer { get; set; }

    public override void Spawned()
    {
        // Chỉ máy có quyền (thường là Server/Host) mới thiết lập bộ đếm
        if (Object.HasStateAuthority)
        {
            destroyTimer = TickTimer.CreateFromSeconds(Runner, lifeTime);
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Kiểm tra nếu thời gian đã hết
        if (Object.HasStateAuthority && destroyTimer.Expired(Runner))
        {
            // Reset timer để không gọi Despawn liên tục nhiều lần
            destroyTimer = TickTimer.None;

            // Lệnh xóa object khỏi server và mọi máy khách
            Runner.Despawn(Object);
        }
    }
}