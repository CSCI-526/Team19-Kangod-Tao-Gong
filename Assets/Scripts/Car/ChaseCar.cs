using UnityEngine;

[DisallowMultipleComponent]
public sealed class ChaseCar : MonoBehaviour
{
    private const float SampleStep = 2f;

    [Header("References")]
    [SerializeField] private InfiniteRoad road;
    [SerializeField] private Transform player;

    [Header("Following")]
    [SerializeField, Min(0f)] private float followDistance = 10f;
    [SerializeField, Min(0f)] private float maxSpeed = 20f;
    [SerializeField, Min(0f)] private float turnSharpness = 8f;
    [SerializeField, Min(0f)] private float edgeMargin = 1f;

    [Header("Weaving")]
    [SerializeField, Min(0f)] private float weaveAmplitude = 2f;
    [SerializeField, Min(0.01f)] private float weaveSpeed = 0.5f;
    [SerializeField] private float weavePhase = 0f;

    private float heightOffset;


    private void Start()
    {
        if (!HasValidSetup())
        {
            enabled = false;
            return;
        }

        if (road.TryGetPathPoint(transform.position, out RoadPathPoint start))
        {
            heightOffset = Vector3.Dot(transform.position - start.Position, start.Up);
        }
    }

    private void Update()
    {
        if (!road.TryGetPathPoint(player.position, out RoadPathPoint playerPoint))
        {
            return;
        }

        if (!TryGetPointBehind(playerPoint, followDistance, out RoadPathPoint target))
        {
            return;
        }

        float weave = Mathf.Sin((Time.time + weavePhase) * weaveSpeed) * weaveAmplitude;
        float limit = Mathf.Max(0f, target.Width * 0.5f - edgeMargin);
        float lateralOffset = Mathf.Clamp(weave, -limit, limit);

        Vector3 targetPosition = target.Position + target.Right * lateralOffset + target.Up * heightOffset;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, maxSpeed * Time.deltaTime);

        Quaternion targetRotation = Quaternion.LookRotation(target.Forward, target.Up);
        float blend = 1f - Mathf.Exp(-turnSharpness * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, blend);

    }

    private bool HasValidSetup()
    {
        if (road == null)
        {
            LogError("road reference is missing.");
            return false;
        }

        if (player == null)
        {
            LogError("player reference is missing.");
            return false;
        }

        return true;
    }

    private bool TryGetPointBehind(RoadPathPoint from, float distance, out RoadPathPoint result)
    {
        result = from;
        float travelled = 0f;

        while (travelled < distance)
        {
            float step = Mathf.Min(SampleStep, distance - travelled);

            if (!road.TryGetPathPoint(result.Position - result.Forward * step, out result))
            {
                return false;
            }

            travelled += step;
        }

        return true;
    }

    private void LogError(string message)
    {
        Debug.LogError($"{nameof(ChaseCar)}: {message}", this);
    }
}