using Fusion;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BulletFusion : NetworkBehaviour
{
    public float tocDo = 20f;
    public float thoiGianTonTai = 5f;
    public int satThuong = 10;

    [Networked] private TickTimer boDemHuy { get; set; }
    [Networked] private Vector3 huongBay { get; set; }

    private Rigidbody rb;

    // Gọi khi spawn để set hướng bay
    public void Init(Vector3 huong)
    {
        huongBay = huong;
    }

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Chỉ Host set timer
        if (Object.HasStateAuthority)
        {
            boDemHuy = TickTimer.CreateFromSeconds(Runner, thoiGianTonTai);
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Di chuyển
        transform.position += huongBay * tocDo * Runner.DeltaTime;

        // Hết thời gian thì biến mất
        if (Object.HasStateAuthority && boDemHuy.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Object.HasStateAuthority) return;
        if (other == null) return;

        // 🔥 Lấy PlayerHealth từ parent (fix lỗi collider con)
        PlayerHealthFusion mau = other.GetComponentInParent<PlayerHealthFusion>();

        if (mau != null)
        {
            // 🔥 Gây damage an toàn
            try
            {
                if (mau.IsAlive())
                {
                    mau.RPC_RequestTakeDamage(satThuong);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Lỗi PlayerHealthFusion: " + e.Message);
            }

            // Tắt Collider ngay lập tức để đạn không vô tình chạm và gây dame nhiều lần
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            // Xóa refapNetwork trong 0.3f khi chạm vào (Dùng TickTimer của Fusion)
            boDemHuy = TickTimer.CreateFromSeconds(Runner, 0.3f);

            return;
        }

        // 🔥 Nếu trúng môi trường
        int mask = LayerMask.GetMask("Default", "Environment");

        if ((mask & (1 << other.gameObject.layer)) != 0)
        {
            // Thêm 1 dòng cho nó tự xóa trong 4f khi trúng tường/vật cản
            boDemHuy = TickTimer.CreateFromSeconds(Runner, 4f);
        }

        // Debug xem đang đụng cái gì
        Debug.Log("Đạn chạm: " + other.name);
    }
}