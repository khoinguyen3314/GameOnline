using UnityEngine;
using Fusion;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class AttackSoi : NetworkBehaviour
{
    [Header("Cấu hình hình nón 3D")]
    public float khoangCachPhatHien = 10f;
    [Range(0, 360)] public float gocPhatHien = 70f;
    [Range(-90, 90)] public float gocCuiXuong = 15f; // Chỉnh góc này để tia cắm xuống đất
    public float chieuCaoMat = 1.5f; // Kéo tia lên ngang đầu con sói
    public int soLuongTia = 40;

    [Header("Layer")]
    public LayerMask layerPlayer;
    public LayerMask layerTuong;

    [Header("Hiển thị View Game")]
    public Color mauBinhThuong = new Color(1f, 1f, 0f, 0.3f);
    public Color mauPhatHien = new Color(1f, 0f, 0f, 0.4f);

    public Material materialHinhNon;

    private Mesh meshHinhNon;
    private MeshRenderer meshRenderer;

    [Networked] private NetworkBool DaThayPlayer { get; set; }

    public override void Spawned()
    {
        meshHinhNon = new Mesh();
        meshHinhNon.name = "ViewConeMesh3D";
        GetComponent<MeshFilter>().mesh = meshHinhNon;

        meshRenderer = GetComponent<MeshRenderer>();
        if (materialHinhNon != null)
        {
            meshRenderer.material = materialHinhNon;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;
        DaThayPlayer = KiemTraPhatHien();
    }

    public override void Render()
    {
        VeMeshHinhNon();

        if (meshRenderer != null && meshRenderer.material != null)
        {
            Color mauHienTai = DaThayPlayer ? mauPhatHien : mauBinhThuong;

            // Dành cho chuẩn Unity cũ (phòng hờ)
            meshRenderer.material.color = mauHienTai;

            // Dành cho chuẩn URP (Universal Render Pipeline)
            if (meshRenderer.material.HasProperty("_BaseColor"))
            {
                meshRenderer.material.SetColor("_BaseColor", mauHienTai);
            }

            // Nếu bật Emission (phát sáng), đổi màu cả cái viền sáng
            if (meshRenderer.material.HasProperty("_EmissionColor"))
            {
                // Nhân 2f để ánh sáng rực lên một chút cho đẹp
                meshRenderer.material.SetColor("_EmissionColor", mauHienTai * 2f);
            }
        }
    }

    // --- HÀM TÍNH TOÁN VỊ TRÍ MẮT VÀ HƯỚNG TIA 3D ---
    Vector3 LayViTriMat()
    {
        // Nâng điểm bắt đầu lên theo chiều cao mắt
        return transform.position + transform.up * chieuCaoMat;
    }

    Vector3 LayHuongTia(float gocY)
    {
        // gocCuiXuong (Trục X), gocY (Trục Y)
        Quaternion localRot = Quaternion.Euler(gocCuiXuong, gocY, 0);
        return transform.rotation * localRot * Vector3.forward;
    }

    bool KiemTraPhatHien()
    {
        Vector3 viTriMat = LayViTriMat();
        float nuaGoc = gocPhatHien / 2f;
        float step = gocPhatHien / soLuongTia;
        LayerMask layerGop = layerPlayer | layerTuong;

        for (int i = 0; i <= soLuongTia; i++)
        {
            float gocY = -nuaGoc + step * i;
            Vector3 huong = LayHuongTia(gocY);

            if (Physics.Raycast(viTriMat, huong, out RaycastHit trung, khoangCachPhatHien, layerGop))
            {
                if (((1 << trung.collider.gameObject.layer) & layerPlayer) != 0) return true;
            }
        }
        return false;
    }

    void VeMeshHinhNon()
    {
        int vertexCount = soLuongTia + 2;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 6];

        Vector3 viTriMatWorld = LayViTriMat();

        // Chuyển vị trí mắt từ World Space về Local Space để làm đỉnh nhọn của Mesh
        vertices[0] = transform.InverseTransformPoint(viTriMatWorld);

        float nuaGoc = gocPhatHien / 2f;
        float step = gocPhatHien / soLuongTia;

        for (int i = 0; i <= soLuongTia; i++)
        {
            float gocY = -nuaGoc + step * i;
            Vector3 huong = LayHuongTia(gocY);

            Vector3 diemCuoi = viTriMatWorld + huong * khoangCachPhatHien;

            if (Physics.Raycast(viTriMatWorld, huong, out RaycastHit trung, khoangCachPhatHien, layerTuong))
            {
                diemCuoi = trung.point;
            }

            vertices[i + 1] = transform.InverseTransformPoint(diemCuoi);

            if (i < soLuongTia)
            {
                int triIndex = i * 6;
                triangles[triIndex] = 0;
                triangles[triIndex + 1] = i + 1;
                triangles[triIndex + 2] = i + 2;
                triangles[triIndex + 3] = 0;
                triangles[triIndex + 4] = i + 2;
                triangles[triIndex + 5] = i + 1;
            }
        }

        meshHinhNon.Clear();
        meshHinhNon.vertices = vertices;
        meshHinhNon.triangles = triangles;
        meshHinhNon.RecalculateNormals();
        meshHinhNon.RecalculateBounds(); // Cập nhật ranh giới 3D
    }

    void OnDrawGizmos()
    {
        Vector3 viTriMat = LayViTriMat();
        float nuaGoc = gocPhatHien / 2f;
        float step = gocPhatHien / soLuongTia;

        Gizmos.color = Color.yellow;

        for (int i = 0; i <= soLuongTia; i++)
        {
            float gocY = -nuaGoc + step * i;
            Vector3 huong = LayHuongTia(gocY);
            Gizmos.DrawRay(viTriMat, huong * khoangCachPhatHien);
        }
    }
}