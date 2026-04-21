using UnityEngine;
using Fusion;

public class SlashDamage : NetworkBehaviour
{
    public int damage = 10;
    public bool hasHit = false;

    public void OnEnable()
    {
        hasHit = false; // reset mỗi lần chém
    }

    public void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        //if (!Object.HasStateAuthority) return;

        var damageable = other.GetComponent<DamageableFusion>();
        if (damageable != null)
        {
            damageable.InflictDamage(damage, gameObject);
            hasHit = true;
        }
    }
}