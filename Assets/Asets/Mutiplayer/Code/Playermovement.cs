using Fusion;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 5f;

    private Animator _animator;
    private CharacterController _controller;

    [Networked] private float _verticalVelocity { get; set; }
    [Networked, OnChangedRender(nameof(OnSpeedChanged))] public float Speed { get; set; }
    [Networked] private Vector3 _impactVelocity { get; set; }

    private void OnSpeedChanged()
    {
        if (_animator != null)
            _animator.SetFloat("Speed", Speed);
    }

    public void ApplyKnockback(Vector3 force)
    {
        _impactVelocity = force;
    }

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }
    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            LockCursor();
        }
    }

    public override void FixedUpdateNetwork()
    {
        Physics.SyncTransforms();

        if (_controller.isGrounded)
            _verticalVelocity = -2f;
        else
            _verticalVelocity += Physics.gravity.y * Runner.DeltaTime;

        if (_impactVelocity.sqrMagnitude > 0.01f)
            _impactVelocity = Vector3.Lerp(_impactVelocity, Vector3.zero, Runner.DeltaTime * 5f);
        else
            _impactVelocity = Vector3.zero;

        var moveDir = Vector3.zero;

        if (GetInput<NetworkInputData>(out var input))
        {
            if (input.moveDirection.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(input.moveDirection),
                    Runner.DeltaTime * 15f);

            if (_controller.isGrounded && input.buttons.IsSet((int)PlayerInputButtons.Jump))
                _verticalVelocity = jumpForce;

            moveDir = input.moveDirection * moveSpeed;
            Speed = input.moveDirection.magnitude;
        }

        Vector3 finalMove = moveDir
                          + new Vector3(0f, _verticalVelocity, 0f)
                          + _impactVelocity;

        _controller.Move(finalMove * Runner.DeltaTime);
    }
}