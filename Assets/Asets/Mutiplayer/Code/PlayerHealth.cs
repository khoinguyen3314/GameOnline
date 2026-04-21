using Fusion;
using Photon.Pun.Demo.PunBasics;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthFusion : NetworkBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;

    [Networked, OnChangedRender(nameof(OnHealthChanged))]
    public int CurrentHealth { get; set; }

    [Networked]
    public NetworkBool IsDead { get; set; }

    [Header("Optional UI")]
    public Slider worldHpBar;
    public GameObject deadFx;

    private bool isInitialized = false;

    // ================= PLAYER MANAGER =================

    void OnEnable()
    {
        if (!PlayerManager.Players.Contains(this))
            PlayerManager.Players.Add(this);
    }

    void OnDisable()
    {
        PlayerManager.Players.Remove(this);
    }

    // ================= SPAWN =================

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            CurrentHealth = maxHealth;
            IsDead = false;
        }

        Invoke(nameof(InitUI), 0.05f);
    }

    void InitUI()
    {
        isInitialized = true;
        OnHealthChanged();
    }

    public bool IsAlive()
    {
        return !IsDead && CurrentHealth > 0;
    }

    // ================= DAMAGE =================

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestTakeDamage(int damage)
    {
        if (!Object.HasStateAuthority) return;
        if (IsDead) return;

        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);

        Debug.Log($"[HP] {Object.Id} -{damage} → {CurrentHealth}");

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    // ================= DIE =================

    void Die()
    {
        if (IsDead) return;

        IsDead = true;

        Debug.Log($"[HP] {Object.Id} chết");

        var controller = GetComponent<CharacterController>();
        if (controller != null)
            controller.enabled = false;
    }

    // ================= UI =================

    void OnHealthChanged()
    {
        if (!isInitialized) return;

        if (worldHpBar != null)
        {
            worldHpBar.maxValue = maxHealth;
            worldHpBar.value = CurrentHealth;
        }

        if (deadFx != null)
        {
            deadFx.SetActive(CurrentHealth <= 0);
        }
    }

    public override void Render()
    {
        OnHealthChanged();
    }
}