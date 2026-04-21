using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCombat : NetworkBehaviour
{
    private AudioSource _audioSource;
    private Animator _animator;

    [Header("Audio")]
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip jumpClip;

    [Header("UI")]
    [SerializeField] private Slider healthSlider;

    [Header("Attack Settings")]
    [SerializeField] private int attackDamage = 10;

    [Header("VFX")]
    [SerializeField] private NetworkPrefabRef slashVFXPrefab;
    [SerializeField] private Transform attackPoint;

    [Networked, OnChangedRender(nameof(OnHealthChanged))]
    public int Health { get; set; }

    private float _localAttackCooldown;
    private const float AttackCooldownTime = 0.6f;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _animator = GetComponent<Animator>();
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            Health = 100;
        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = 100;
            healthSlider.value = Health;
        }
    }

    private void OnHealthChanged()
    {
        if (healthSlider != null)
            healthSlider.value = Health;
    }

    public bool CanAttack()
    {
        return Time.time >= _localAttackCooldown;
    }

    public void Attack()
    {
        if (!CanAttack()) return;

        Rpc_Attack();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void Rpc_Attack()
    {
        // Sound
        if (_audioSource && attackClip)
            _audioSource.PlayOneShot(attackClip);

        // Animation
        if (_animator)
            _animator.SetTrigger("Attack");

        // Cooldown (host)
        if (Object.HasStateAuthority)
            _localAttackCooldown = Time.time + AttackCooldownTime;

        // Spawn VFX
        SpawnSlashVFX();
    }

    private void SpawnSlashVFX()
    {
        if (slashVFXPrefab == null || attackPoint == null) return;

        if (Object.HasStateAuthority)
        {
            Runner.Spawn(
                slashVFXPrefab,
                attackPoint.position,
                attackPoint.rotation,
                Object.InputAuthority
            );
        }
    }

    // ? FIX l?i b?n g?p
    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void Rpc_Jump()
    {
        if (_audioSource && jumpClip)
            _audioSource.PlayOneShot(jumpClip);

        if (_animator)
            _animator.SetTrigger("Jump");
    }
}