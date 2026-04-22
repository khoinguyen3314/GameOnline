using UnityEngine;
using UnityEngine.AI;
using Fusion;

public class Run1 : NetworkBehaviour
{
    Animator ani;
    private Transform nguoiChoiMucTieu;
    public float tocDo = 3.5f;

    [Header("Cài đặt AI bỏ chạy")]
    public float khoangCachBoChay = 5f;
    public float khoangCachDiChuyen = 10f;
    [SerializeField] NetworkBehaviour TanCong;
    private NavMeshAgent aiDiChuyen;

    public override void Spawned()
    {
        base.Spawned();
        ani = GetComponent<Animator>();
        aiDiChuyen = GetComponent<NavMeshAgent>();

        if (aiDiChuyen != null)
        {
            aiDiChuyen.speed = tocDo;
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Chỉ máy Host (Server) mới tính toán AI để đồng bộ cho các máy khác
        if (Object == null || !Object.HasStateAuthority) return;

        if (aiDiChuyen == null) return;

        // [QUAN TRỌNG] Sửa lỗi quái lơ lửng không nhận Navmesh lúc mới Spawn
        if (!aiDiChuyen.isOnNavMesh)
        {
            NavMeshHit hitInfo;
            // Tìm điểm trên NavMesh gần nhất trong bán kính 2f và ép quái xuống đó
            if (NavMesh.SamplePosition(transform.position, out hitInfo, 2.0f, NavMesh.AllAreas))
            {
                
                aiDiChuyen.Warp(hitInfo.position);
            }
            return; // Dừng frame này để đợi Navmesh Agent kết nối xong
        }

        TimNguoiChoiGanNhat();

        // Nếu không có ai gần đó thì đứng im và ĐẢM BẢO bật lại Script tấn công
        if (nguoiChoiMucTieu == null)
        {
            ani.SetBool("Run", false);
            if (TanCong != null && !TanCong.enabled)
            {
                TanCong.enabled = true;
            }
            return;
        }

        float khoangCach = Vector3.Distance(transform.position, nguoiChoiMucTieu.position);

        if (khoangCach <= khoangCachBoChay)
        {
            // 🔥 Người chơi trong tầm -> TẮT TẤN CÔNG & BỎ CHẠY
            if (TanCong != null && TanCong.enabled)
            {
                ani.SetBool("Run", true);
                TanCong.enabled = false;
            }

            // Tính hướng chạy: từ Player đẩy ra xa Quái
            Vector3 huongChay = (transform.position - nguoiChoiMucTieu.position).normalized;
            Vector3 diemDichDuKien = transform.position + huongChay * khoangCachDiChuyen;

            NavMeshHit hit;
            // Đảm bảo điểm quái bỏ chạy đến vẫn nằm trên bản đồ di chuyển được
            if (NavMesh.SamplePosition(diemDichDuKien, out hit, 2f, NavMesh.AllAreas))
            {
                aiDiChuyen.SetDestination(hit.position);
            }
        }
        else
        {
            // 🔥 Người chơi ngoài tầm -> ĐỨNG LẠI & BẬT TẤN CÔNG
            if (aiDiChuyen.hasPath)
            {
                aiDiChuyen.ResetPath(); // An toàn rồi thì đứng im
            }

            if (TanCong != null && !TanCong.enabled)
            {
                TanCong.enabled = true;
            }
        }
    }

    // Tối ưu hóa: Dùng OverlapSphere thay cho FindGameObjectsWithTag
    private void TimNguoiChoiGanNhat()
    {
        // Chỉ quét các vật thể nằm trong phạm vi (khoangCachBoChay + bù hao 1 chút)
        Collider[] cacVatTheXungQuanh = Physics.OverlapSphere(transform.position, khoangCachBoChay + 2f);

        float khoangCachNganNhat = Mathf.Infinity;
        Transform mucTieuGanNhat = null;

        foreach (Collider col in cacVatTheXungQuanh)
        {
            // Player của bạn đang dùng Character Controller (đóng vai trò như 1 Collider) và tag "Player"
            if (col.CompareTag("Player"))
            {
                float khoangCach = Vector3.Distance(transform.position, col.transform.position);
                if (khoangCach < khoangCachNganNhat)
                {
                    khoangCachNganNhat = khoangCach;
                    mucTieuGanNhat = col.transform;
                }
            }
        }

        nguoiChoiMucTieu = mucTieuGanNhat;
    }

    //Hàm này giúp bạn nhìn thấy bán kính bỏ chạy của quái(Màu đỏ) trong màn hình Scene của Unity
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, khoangCachBoChay);
    }
}