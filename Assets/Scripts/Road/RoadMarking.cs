using UnityEngine;

[DisallowMultipleComponent]
public sealed class RoadMarking : MonoBehaviour
{
    private const int LaneCount = 3;

    [Header("References")]
    [SerializeField] private RoadSegment road;
    [SerializeField] private GameObject stripMarking;
    [SerializeField] private Material stripMaterial;

    [Header("Strip Settings")]
    [SerializeField, Min(0.01f)] private float stripWidth = 0.15f;
    [SerializeField, Min(0.1f)] private float stripLength = 2f;
    [SerializeField, Min(0f)] private float gapLength = 2f;
    [SerializeField, Min(0f)] private float height = 0.01f;

    private void Awake()
    {
        GenerateMarkings();
    }

    private void Reset()
    {
        road = GetComponentInParent<RoadSegment>();
    }

    private void GenerateMarkings()
    {
        if (!HasValidSetup() || !TryGetStripScale(out Vector3 stripScale))
        {
            return;
        }

        Transform holder = new GameObject("RoadMarkings").transform;
        holder.SetParent(road.transform, false);

        float pathLength = GetPathLength();
        float spacing = stripLength + gapLength;
        float dividerOffset = road.Width / LaneCount * 0.5f;
        int stripCount = Mathf.FloorToInt((pathLength - stripLength) / spacing + 0.0001f) + 1;

        for (int i = 0; i < stripCount; i++)
        {
            RoadPathPoint point = SamplePath(i * spacing + stripLength * 0.5f);

            CreateStrip(holder, point, -dividerOffset, stripScale);
            CreateStrip(holder, point, dividerOffset, stripScale);
        }
    }

    private bool HasValidSetup()
    {
        if (road == null)
        {
            LogError("road segment reference is missing.");
            return false;
        }

        if (!road.IsConfigured)
        {
            LogError("the road segment is missing its start point, end point or a waypoint.", road);
            return false;
        }

        if (stripMarking == null)
        {
            LogError("strip plane prefab reference is missing.");
            return false;
        }

        return true;
    }

    private bool TryGetStripScale(out Vector3 scale)
    {
        scale = Vector3.zero;

        if (!stripMarking.TryGetComponent(out MeshFilter meshFilter) || meshFilter.sharedMesh == null)
        {
            LogError("the strip plane prefab needs a MeshFilter with a mesh.", stripMarking);
            return false;
        }

        Vector3 meshSize = meshFilter.sharedMesh.bounds.size;

        if (meshSize.x <= Mathf.Epsilon || meshSize.z <= Mathf.Epsilon)
        {
            LogError("the strip plane mesh has no width or length.", stripMarking);
            return false;
        }

        scale = new Vector3(stripWidth / meshSize.x, 1f, stripLength / meshSize.z);
        return true;
    }

    private void CreateStrip(Transform parent, RoadPathPoint point, float lateralOffset, Vector3 scale)
    {
        GameObject strip = Instantiate(stripMarking, parent);

        Transform stripTransform = strip.transform;
        stripTransform.SetPositionAndRotation(
            point.Position + point.Right * lateralOffset + point.Up * height,
            Quaternion.LookRotation(point.Forward, point.Up));
        stripTransform.localScale = scale;

        if (strip.TryGetComponent(out Renderer stripRenderer))
        {
            stripRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            if (stripMaterial != null)
            {
                stripRenderer.sharedMaterial = stripMaterial;
            }
        }

        if (strip.TryGetComponent(out Collider stripCollider))
        {
            Destroy(stripCollider);
        }
    }

    private float GetPathLength()
    {
        float length = 0f;
        Vector3 from = road.GetPathPoint(0);

        for (int i = 1; i < road.PathPointCount; i++)
        {
            Vector3 to = road.GetPathPoint(i);
            length += Vector3.Distance(from, to);
            from = to;
        }

        return length;
    }

    private RoadPathPoint SamplePath(float distance)
    {
        Vector3 from = road.GetPathPoint(0);
        Vector3 direction = Vector3.forward;
        float remaining = distance;

        for (int i = 1; i < road.PathPointCount; i++)
        {
            Vector3 to = road.GetPathPoint(i);
            Vector3 delta = to - from;
            float length = delta.magnitude;

            if (length > Mathf.Epsilon)
            {
                direction = delta / length;

                if (remaining <= length)
                {
                    return new RoadPathPoint(from + direction * remaining, direction, road.Width);
                }

                remaining -= length;
            }

            from = to;
        }

        return new RoadPathPoint(from, direction, road.Width);
    }

    private void LogError(string message, Object context = null)
    {
        Debug.LogError($"{nameof(RoadMarking)}: {message}", context != null ? context : this);
    }
}