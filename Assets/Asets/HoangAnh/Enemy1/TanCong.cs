using UnityEngine;
using Fusion;

public class EnemyAttackFusion : NetworkBehaviour
{
    [Header("Cài đặt tấn công")]
    public float banKinhTanCong = 7f;
    public float thoiGianHoiChieu = 1.5f;

    [Header("Prefab đạn")]
    public NetworkPrefabRef danPrefab;

    [Header("Điểm bắn")]
    public Transform diemBan;

    private double lanBanCuoi;
    private Transform mucTieu;
    Animator ani;
    public override void Spawned()
    {
        base.Spawned();
        ani = GetComponent<Animator>();
    }
    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        TimMucTieuGanNhat();

        if (mucTieu == null) return;

        float kc = Vector3.Distance(transform.position, mucTieu.position);

        if (kc <= banKinhTanCong)
        {
            // 🔥 Xoay mặt
            Vector3 huong = (mucTieu.position - transform.position);
            huong.y = 0;

            if (huong != Vector3.zero)
            {
                Quaternion rot = Quaternion.LookRotation(huong);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, Runner.DeltaTime * 5f);
            }

            // 🔥 Bắn
            if (Runner.SimulationTime - lanBanCuoi >= thoiGianHoiChieu)
            {
                lanBanCuoi = Runner.SimulationTime;
                BanDan();
            }
        }
    }

    void BanDan()
    {
        if (danPrefab == null || diemBan == null || mucTieu == null) return;

        Vector3 huongBan = (mucTieu.position - diemBan.position).normalized;        
        NetworkObject dan = Runner.Spawn(
            danPrefab,
            diemBan.position,
            Quaternion.LookRotation(huongBan),
            Object.InputAuthority
        );

        if (dan != null)
        {
            BulletFusion bullet = dan.GetComponent<BulletFusion>();
            if (bullet != null)
            {
                ani.SetTrigger("Attack");
                bullet.Init(huongBan);
            }
        }
    }

    void TimMucTieuGanNhat()
    {
        Collider[] list = Physics.OverlapSphere(transform.position, banKinhTanCong);

        float min = Mathf.Infinity;
        Transform ganNhat = null;

        foreach (var col in list)
        {
            if (col.CompareTag("Player"))
            {
                float d = Vector3.Distance(transform.position, col.transform.position);
                if (d < min)
                {
                    min = d;
                    ganNhat = col.transform;
                }
            }
        }

        mucTieu = ganNhat;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, banKinhTanCong);
    }
}