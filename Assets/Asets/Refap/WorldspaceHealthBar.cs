using UnityEngine;
using UnityEngine.UI;

public class WorldspaceHealthBar : MonoBehaviour
{
    public Slider healthSlider;
    public Transform HealthBarPivot;
    public bool HideFullHealthBar = true;

    void LateUpdate()
    {
        if (CameraHolder.Cam == null) return;

        // luôn quay về phía camera
        HealthBarPivot.forward = CameraHolder.Cam.transform.forward;
    }

    // 🔥 Update máu
    public void UpdateBar(int current, int max)
    {
        if (healthSlider == null) return;

        healthSlider.maxValue = max;
        healthSlider.value = current;

        if (HideFullHealthBar)
        {
            HealthBarPivot.gameObject.SetActive(current < max);
        }
    }
}