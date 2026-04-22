using UnityEngine;

public class PatrolPointManager : MonoBehaviour
{
    public static PatrolPointManager Instance;

    public Transform[] points;

    private void Awake()
    {
        Instance = this;
    }
}