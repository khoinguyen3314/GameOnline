using Fusion;
using UnityEngine;

public class ChetQuaiVat : NetworkBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (!Object.HasStateAuthority) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            NetworkObject netObj = collision.gameObject.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                Runner.Despawn(netObj);
            }
        }
    }
}