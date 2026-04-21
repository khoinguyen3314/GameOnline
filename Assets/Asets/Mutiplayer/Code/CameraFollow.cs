using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    public void AssignCamera(Transform target)
    {
        GetComponent<CinemachineCamera>().Target.TrackingTarget = target;
    }

    void Update()
    {
        if (Mouse.current == null) return;

        float scrollValue = Mouse.current.scroll.y.ReadValue();
        if (scrollValue == 0f) return;

        var orbital = GetComponent<CinemachineOrbitalFollow>();
        if (orbital == null) return;

        orbital.Radius -= scrollValue / 120f;
        orbital.Radius = Mathf.Clamp(orbital.Radius, 2f, 12f);
    }
}