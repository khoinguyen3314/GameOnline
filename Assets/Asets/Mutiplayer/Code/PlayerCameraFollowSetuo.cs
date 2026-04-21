using Fusion;
using UnityEngine;

public class PlayerCameraSetup : NetworkBehaviour
{
    public override void Spawned()
    {
        if (!HasInputAuthority) return;

        var cameraFollow = FindAnyObjectByType<CameraFollow>();

        if (cameraFollow != null)
        {
            cameraFollow.AssignCamera(transform);

            // ?? L?y camera hi?n t?i
            CameraHolder.Cam = cameraFollow.GetComponentInChildren<Camera>();
        }
    }
}