using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;
    private void LateUpdate()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        if (mainCamera != null)
        {
            transform.forward = mainCamera.transform.forward;
        }
}
}
