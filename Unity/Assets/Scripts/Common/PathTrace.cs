using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public sealed class PathTrace : MonoBehaviour
{
    [SerializeField] private float minDistance = 0.1f;
    [SerializeField] private int maxPoints = 500;

    private LineRenderer lineRenderer;
    private readonly List<Vector3> points = new List<Vector3>();

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 0;
    }

    private void LateUpdate()
    {
        Vector3 position = transform.position;

        if (points.Count == 0 || (position - points[^1]).sqrMagnitude >= minDistance * minDistance)
        {
            points.Add(position);
            if (points.Count > maxPoints) points.RemoveAt(0);

            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
        }
    }
}
