using UnityEngine;

public class Path : MonoBehaviour
{
    public Transform[] points;

    public int resolution = 20;
    public int gizmoResolution = 50;
    public int laneCount = 3;
    public float laneSpacing = 0.35f;

    float[] cumulativeLengths;
    float totalLength;

    void Awake()
    {
        if (points != null && points.Length >= 2)
            PrecomputeLengths();
    }

    void PrecomputeLengths()
    {
        int samples = (points.Length - 1) * resolution;
        cumulativeLengths = new float[samples + 1];

        Vector2 prev = GetPointRaw(0);
        totalLength = 0f;
        cumulativeLengths[0] = 0f;

        for (int i = 1; i <= samples; i++)
        {
            float t = (float)i / samples * (points.Length - 1);
            Vector2 p = GetPointRaw(t);
            totalLength += Vector2.Distance(prev, p);
            cumulativeLengths[i] = totalLength;
            prev = p;
        }
    }

    Vector2 GetPointRaw(float t)
    {
        if (points == null || points.Length < 2)
            return Vector2.zero;

        int count = points.Length;
        int i = Mathf.Clamp(Mathf.FloorToInt(t), 0, count - 2);
        float lt = t - i;

        Vector2 p0 = points[Mathf.Clamp(i - 1, 0, count - 1)]?.position ?? Vector2.zero;
        Vector2 p1 = points[i]?.position ?? Vector2.zero;
        Vector2 p2 = points[i + 1]?.position ?? Vector2.zero;
        Vector2 p3 = points[Mathf.Clamp(i + 2, 0, count - 1)]?.position ?? Vector2.zero;

        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * lt +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * lt * lt +
            (-p0 + 3f * p1 - 3f * p2 + p3) * lt * lt * lt
        );
    }

    public Vector2 GetPointAtDistance(float distance)
    {
        if (cumulativeLengths == null || cumulativeLengths.Length == 0)
            PrecomputeLengths();

        distance = Mathf.Clamp(distance, 0, totalLength);

        int index = 1;
        while (index < cumulativeLengths.Length && cumulativeLengths[index] < distance)
            index++;

        float d0 = cumulativeLengths[index - 1];
        float d1 = cumulativeLengths[index];
        float lerp = Mathf.InverseLerp(d0, d1, distance);

        float t0 = (float)(index - 1) / (cumulativeLengths.Length - 1) * (points.Length - 1);
        float t1 = (float)index / (cumulativeLengths.Length - 1) * (points.Length - 1);

        return GetPointRaw(Mathf.Lerp(t0, t1, lerp));
    }

    public Vector2 GetDirectionAtDistance(float distance)
    {
        return (GetPointAtDistance(distance + 0.05f) - GetPointAtDistance(distance)).normalized;
    }

void OnDrawGizmos()
    {
        if (points == null || points.Length < 2)
            return;

        if (cumulativeLengths == null || cumulativeLengths.Length == 0)
            PrecomputeLengths();

        float step = totalLength / gizmoResolution;

        Gizmos.color = Color.green;
        Vector2 prev = GetPointAtDistance(0);
        for (float d = step; d <= totalLength; d += step)
        {
            Vector2 p = GetPointAtDistance(d);
            Gizmos.DrawLine(prev, p);
            prev = p;
        }

        for (int lane = 0; lane < laneCount; lane++)
        {
            float offset = (lane - (laneCount - 1) / 2f) * laneSpacing;
            Gizmos.color = Color.cyan;
            prev = GetLanePoint(0, offset);
            for (float d = step; d <= totalLength; d += step)
            {
                Vector2 p = GetLanePoint(d, offset);
                Gizmos.DrawLine(prev, p);
                prev = p;
            }
        }

        Gizmos.color = Color.red;
        foreach (var pt in points)
        {
            if (pt != null)
                Gizmos.DrawSphere(pt.position, 0.08f);
        }
    }

    Vector2 GetLanePoint(float distance, float offset)
    {
        Vector2 center = GetPointAtDistance(distance);
        Vector2 dir = GetDirectionAtDistance(distance);
        Vector2 perp = new Vector2(-dir.y, dir.x);
        return center + perp * offset;
    }
}
