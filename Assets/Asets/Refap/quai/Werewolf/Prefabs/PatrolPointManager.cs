using UnityEngine;
using Fusion;

public class PatrolPointManager : NetworkBehaviour
{
    [Header("Patrol Points")]
    [SerializeField] private Transform[] points;

    public Transform GetPoint(int index)
    {
        if (points == null || points.Length == 0) return null;
        return points[index % points.Length];
    }

    public int PointCount => points.Length;
}