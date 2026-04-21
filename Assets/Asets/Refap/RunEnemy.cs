using UnityEngine;
using UnityEngine.AI; // Gọi thư viện AI để dùng NavMesh

public class RunEnemy : MonoBehaviour
{
    Animator ani;
    [Header("Mục tiêu")]
    public Transform nguoiChoi;           // Vị trí của người chơi mà quái sẽ đuổi theo

    [Header("Cài đặt Di chuyển")]
    public float tocDo = 3.5f;            // Tốc độ di chuyển của kẻ địch

    // Biến này đóng vai trò là "bộ não" giúp kẻ địch tự tìm đường và né vật cản
    private NavMeshAgent aiDiChuyen;

    void Start()
    {
        ani = GetComponent<Animator>();
        // Lấy thành phần NavMeshAgent đã gắn trên Kẻ địch
        aiDiChuyen = GetComponent<NavMeshAgent>();

        // Gán tốc độ di chuyển
        aiDiChuyen.speed = tocDo;

        // Nếu bạn quên chưa kéo Player vào ô Nguoi Choi, code sẽ tự tìm vật thể có tag "Player"
        if (nguoiChoi == null)
        {
            GameObject timNguoiChoi = GameObject.FindGameObjectWithTag("Player");
            if (timNguoiChoi != null)
            {
                nguoiChoi = timNguoiChoi.transform;
            }
        }
    }

    void Update()
    {
        // Kiểm tra xem đã có mục tiêu (nguoiChoi) chưa và Kẻ địch có đang đứng trên vùng di chuyển (NavMesh) không
        if (nguoiChoi != null && aiDiChuyen.isOnNavMesh)
        {
            ani.SetTrigger("Run");
            // Ra lệnh cho bộ não AI liên tục đi đến vị trí hiện tại của Người chơi.
            // NavMeshAgent sẽ TỰ ĐỘNG tính toán đường đi để né các vật cản trên bản đồ.
            aiDiChuyen.SetDestination(nguoiChoi.position);
        }
    }
}