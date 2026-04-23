using UnityEngine;
using Fusion;

public class DameCanChienQuai : NetworkBehaviour
{
    [Header("Thông số sát thương cận chiến")]
    public int satThuong = 10;

    // Xài OnTriggerEnter: Vừa chạm vào Collider là trừ máu 1 lần
    private void OnTriggerEnter(Collider other)
    {
        // Chỉ thằng nắm quyền (Server/Host) mới được phép xử lý trừ máu
        if (!Object.HasStateAuthority) return;
        if (other == null) return;

        // Lấy script máu của Player
        PlayerHealthFusion mau = other.GetComponentInParent<PlayerHealthFusion>();

        if (mau != null)
        {
            // Gây damage an toàn
            try
            {
                if (mau.IsAlive())
                {
                    mau.RPC_RequestTakeDamage(satThuong);

                    Debug.Log("💥 [CẬN CHIẾN] Quái vừa vả: " + other.name + " | Trừ: " + satThuong + " máu");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("❌ Lỗi PlayerHealthFusion: " + e.Message);
            }
        }
    }
}