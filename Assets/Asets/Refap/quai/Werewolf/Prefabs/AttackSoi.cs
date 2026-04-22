using UnityEngine;
using Fusion;

[RequireComponent(typeof(LineRenderer))]
public class AttackSoi : NetworkBehaviour
{
    [Header("Cấu hình hình nón")]
    [SerializeField] private float khoangCachPhatHien = 6f;
    [SerializeField] private float gocPhatHien = 60f;
    [SerializeField] private int soLuongTia = 20;

    [Header("Layer")]
    [SerializeField] private LayerMask layerPlayer;
    [SerializeField] private LayerMask layerTuong;

    [Header("Debug")]
    [SerializeField] private bool veDebug = true;
    [SerializeField] private float thoiGianDebug = 0.1f;

    [Header("Hiển thị")]
    [SerializeField] private Color mauBinhThuong = Color.yellow;
    [SerializeField] private Color mauPhatHien = Color.red;

    private LineRenderer duongVe;

    public override void Spawned()
    {
        duongVe = GetComponent<LineRenderer>();
        duongVe.positionCount = soLuongTia + 1;
        duongVe.useWorldSpace = true;
        duongVe.widthMultiplier = 0.05f;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        bool thayPlayer = KiemTraVaPhatHien();
        VeHinhNon(thayPlayer);
    }

    // ================= PHÁT HIỆN =================
    bool KiemTraVaPhatHien()
    {
        float nuaGoc = gocPhatHien * 0.5f;
        LayerMask layerGop = layerPlayer | layerTuong;

        bool daThayPlayer = false;

        for (int i = 0; i < soLuongTia; i++)
        {
            float tiLe = (float)i / (soLuongTia - 1);
            float gocHienTai = Mathf.Lerp(-nuaGoc, nuaGoc, tiLe);

            Vector3 huong = Quaternion.Euler(0, gocHienTai, 0) * transform.forward;

            Ray tia = new Ray(transform.position, huong);
            RaycastHit trung;

            if (Physics.Raycast(tia, out trung, khoangCachPhatHien, layerGop))
            {
                // 🧱 Trúng tường → bị chặn
                if (((1 << trung.collider.gameObject.layer) & layerTuong) != 0)
                {
                    if (veDebug)
                        Debug.DrawLine(tia.origin, trung.point, Color.red, thoiGianDebug);
                }
                // 🎯 Trúng player
                else if (((1 << trung.collider.gameObject.layer) & layerPlayer) != 0)
                {
                    daThayPlayer = true;

                    if (veDebug)
                        Debug.DrawLine(tia.origin, trung.point, Color.green, thoiGianDebug);

                    Debug.Log("👁️ Thấy Player: " + trung.collider.name);
                }
            }
            else
            {
                if (veDebug)
                    Debug.DrawRay(tia.origin, huong * khoangCachPhatHien, Color.yellow, thoiGianDebug);
            }
        }

        return daThayPlayer;
    }

    // ================= VẼ HÌNH NÓN =================
    void VeHinhNon(bool thayPlayer)
    {
        float nuaGoc = gocPhatHien * 0.5f;
        LayerMask layerGop = layerPlayer | layerTuong;

        duongVe.positionCount = soLuongTia + 1;
        duongVe.SetPosition(0, transform.position);

        Color mauHienTai = thayPlayer ? mauPhatHien : mauBinhThuong;
        duongVe.startColor = mauHienTai;
        duongVe.endColor = mauHienTai;

        for (int i = 0; i < soLuongTia; i++)
        {
            float tiLe = (float)i / (soLuongTia - 1);
            float gocHienTai = Mathf.Lerp(-nuaGoc, nuaGoc, tiLe);

            Vector3 huong = Quaternion.Euler(0, gocHienTai, 0) * transform.forward;

            Ray tia = new Ray(transform.position, huong);
            RaycastHit trung;

            Vector3 diemCuoi;

            if (Physics.Raycast(tia, out trung, khoangCachPhatHien, layerGop))
            {
                diemCuoi = trung.point; // bị tường cắt
            }
            else
            {
                diemCuoi = transform.position + huong * khoangCachPhatHien;
            }

            duongVe.SetPosition(i + 1, diemCuoi);
        }
    }

    // ================= GIZMOS =================
    void OnDrawGizmos()
    {
        if (!veDebug) return;

        float nuaGoc = gocPhatHien * 0.5f;

        for (int i = 0; i < soLuongTia; i++)
        {
            float tiLe = (float)i / (soLuongTia - 1);
            float gocHienTai = Mathf.Lerp(-nuaGoc, nuaGoc, tiLe);

            Vector3 huong = Quaternion.Euler(0, gocHienTai, 0) * transform.forward;

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, huong * khoangCachPhatHien);
        }
    }
}