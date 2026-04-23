using UnityEngine;
using Fusion;
using UnityEngine.AI;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(NavMeshAgent))]
public class AttackSoi : NetworkBehaviour
{
    // =========================================================
    // 1. CÁC BIẾN CẤU HÌNH TRÊN INSPECTOR
    // =========================================================

    [Header("Vị trí xuất phát tia")]
    [Tooltip("Kéo thả một Transform (VD: Mắt con quái) vào đây. Nếu có cái này, nó sẽ ưu tiên xài điểm này.")]
    [SerializeField] private Transform diemXuatPhatTia;

    [Header("Tinh chỉnh gốc tia (X, Y, Z)")]
    [Tooltip("Nếu không xài điểm kéo thả ở trên, mày có thể chỉnh thủ công: X(Ngang), Y(Cao), Z(Tới/Lui)")]
    public Vector3 doLechViTri = new Vector3(0f, 1.5f, 0f);

    [Header("Cấu hình hình nón 3D")]
    public float khoangCachPhatHien = 10f;
    [Range(0, 360)] public float gocPhatHien = 70f;
    [Range(-90, 90)] public float gocCuiXuong = 15f;
    public int soLuongTia = 40;

    [Header("Layer")]
    public LayerMask layerPlayer;
    public LayerMask layerTuong;

    [Header("Tốc Độ Rượt Đuổi")]
    public float tocDoDuoiBat = 6.0f;

    [Header("Hiển thị View Game")]
    public Color mauBinhThuong = new Color(1f, 1f, 0f, 0.3f);
    public Color mauPhatHien = new Color(1f, 0f, 0f, 0.4f);
    public Material materialHinhNon;

    Animator ani;
    // =========================================================
    // 2. BIẾN NỘI BỘ VÀ ĐỒNG BỘ MẠNG
    // =========================================================

    private Mesh meshHinhNon;
    private MeshRenderer meshRenderer;
    private NavMeshAgent agent;

    private Transform mucTieuBaoDong;
    [SerializeField] private NetworkBehaviour scriptTuanTra;

    [Networked] private NetworkBool DaThayPlayer { get; set; }

    // =========================================================
    // 3. CÁC HÀM VÒNG LẶP CHÍNH
    // =========================================================

    public override void Spawned()
    {
        ani = GetComponent<Animator>();
        meshHinhNon = new Mesh();
        meshHinhNon.name = "ViewConeMesh3D";
        GetComponent<MeshFilter>().mesh = meshHinhNon;

        meshRenderer = GetComponent<MeshRenderer>();
        if (materialHinhNon != null)
        {
            meshRenderer.material = materialHinhNon;
        }

        agent = GetComponent<NavMeshAgent>();
        scriptTuanTra = GetComponent<SoiAiI>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        if (!DaThayPlayer)
        {
            Transform playerVuaThay;
            if (KiemTraPhatHien(out playerVuaThay))
            {
                DaThayPlayer = true;
                mucTieuBaoDong = playerVuaThay;

                // Lọt vào tia Raycast -> Chạy Animation Run
                if (ani != null) ani.SetTrigger("Run");
            }
        }

        if (DaThayPlayer && mucTieuBaoDong != null)
        {
            if (scriptTuanTra != null && scriptTuanTra.enabled)
            {
                scriptTuanTra.enabled = false;
            }

            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.speed = tocDoDuoiBat;
                agent.SetDestination(mucTieuBaoDong.position);

                // Tính khoảng cách tới Player, nếu lọt vô tầm Stopping Distance thì Attack
                float khoangCach = Vector3.Distance(transform.position, mucTieuBaoDong.position);
                if (khoangCach <= agent.stoppingDistance)
                {
                    if (ani != null) ani.SetTrigger("Attack");
                }
            }
        }
        else
        {
            if (scriptTuanTra != null && !scriptTuanTra.enabled)
            {
                scriptTuanTra.enabled = true;
            }
        }
    }

    public override void Render()
    {
        VeMeshHinhNon();

        if (meshRenderer != null && meshRenderer.material != null)
        {
            Color mauHienTai = DaThayPlayer ? mauPhatHien : mauBinhThuong;
            meshRenderer.material.color = mauHienTai;

            if (meshRenderer.material.HasProperty("_BaseColor"))
            {
                meshRenderer.material.SetColor("_BaseColor", mauHienTai);
            }

            if (meshRenderer.material.HasProperty("_EmissionColor"))
            {
                meshRenderer.material.SetColor("_EmissionColor", mauHienTai * 2f);
            }
        }
    }

    // =========================================================
    // 4. CÁC HÀM TÍNH TOÁN
    // =========================================================

    Vector3 LayViTriMat()
    {
        if (diemXuatPhatTia != null)
        {
            return diemXuatPhatTia.position;
        }

        // TransformDirection giúp độ lệch XYZ luôn xoay theo hướng nhìn của con quái
        return transform.position + transform.TransformDirection(doLechViTri);
    }

    Vector3 LayHuongTia(float gocY)
    {
        Quaternion localRot = Quaternion.Euler(gocCuiXuong, gocY, 0);
        return transform.rotation * localRot * Vector3.forward;
    }

    bool KiemTraPhatHien(out Transform viTriPlayer)
    {
        viTriPlayer = null;
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
                if (((1 << trung.collider.gameObject.layer) & layerPlayer) != 0)
                {
                    viTriPlayer = trung.transform;
                    return true;
                }
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
        meshHinhNon.RecalculateBounds();
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