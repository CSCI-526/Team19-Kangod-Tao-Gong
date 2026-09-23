using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[DisallowMultipleComponent]
public sealed class FuelTankSpawner : MonoBehaviour
{
    private const float SampleStep = 5f;

    [Header("References")]
    [SerializeField] private InfiniteRoad road;
    [SerializeField] private Transform car;
    [SerializeField] private GameObject fuelTankPrefab;

    [Header("Spawning")]
    [SerializeField, Min(1f)] private float spawnDistance = 80f;
    [SerializeField, Min(1f)] private float minSpacing = 60;
    [SerializeField, Min(1f)] private float maxSpacing = 180;
    [SerializeField, Range(0f, 1f)] private float doubleRowChance = 0f;
    [SerializeField, Min(0f)] private float minRowSeparation = 60f;
    [SerializeField, Min(0f)] private float edgeMargin = 1f;
    [SerializeField, Min(0f)] private float despawnDistance = 15f;

    [Header("Clearance")]
    [SerializeField, Min(0f)] private float clearanceRadius = 1.5f;
    [SerializeField, Min(1)] private int placementAttempts = 12;
    [SerializeField, Min(1f)] private float retrySpacing = 5f;

    [Header("Pickup Motion")]
    [SerializeField, Min(0f)] private float bobHeight = 0.12f;
    [SerializeField, Min(0f)] private float bobSpeed = 2.5f;
    [SerializeField, Min(0f)] private float flipSpeed = 90f;

    private readonly List<FuelTank> activeFuelTanks = new List<FuelTank>();
    private ObjectPool<FuelTank> pool;
    private Vector3 previousCarPosition;
    private float distanceUntilSpawn;
    private readonly Collider[] overlapBuffer = new Collider[16];

    private void Awake()
    {
        if (!HasValidSetup())
        {
            enabled = false;
            return;
        }

        pool = new ObjectPool<FuelTank>(
            createFunc: () => new FuelTank(Instantiate(fuelTankPrefab, transform)),
            actionOnGet: FuelTank => FuelTank.GameObject.SetActive(true),
            actionOnRelease: FuelTank => FuelTank.GameObject.SetActive(false),
            actionOnDestroy: FuelTank => Destroy(FuelTank.GameObject));

        previousCarPosition = car.position;
    }

    private void Update()
    {
        RecycleBehindCar();

        distanceUntilSpawn -= Vector3.Distance(car.position, previousCarPosition);
        previousCarPosition = car.position;

        while (distanceUntilSpawn <= 0f)
        {
            distanceUntilSpawn += TrySpawn() ? Random.Range(minSpacing, maxSpacing) : retrySpacing;
        }
    }

    public bool TryCollect(Transform hit)
    {
        for (int i = activeFuelTanks.Count - 1; i >= 0; i--)
        {
            FuelTank FuelTank = activeFuelTanks[i];

            if (!hit.IsChildOf(FuelTank.Transform))
            {
                continue;
            }

            pool.Release(FuelTank);
            activeFuelTanks.RemoveAt(i);
            return true;
        }

        return false;
    }

    private bool HasValidSetup()
    {
        if (road == null)
        {
            LogError("road reference is missing.");
            return false;
        }

        if (car == null)
        {
            LogError("car reference is missing.");
            return false;
        }

        if (fuelTankPrefab == null)
        {
            LogError("FuelTank prefab reference is missing.");
            return false;
        }

        if (fuelTankPrefab.GetComponentInChildren<Renderer>() == null)
        {
            LogError("the FuelTank prefab needs a Renderer.", fuelTankPrefab);
            return false;
        }

        return true;
    }

    private bool TrySpawn()
    {
        if (!TryGetPointAhead(out RoadPathPoint point))
        {
            return false;
        }

        float limit = Mathf.Max(0f, point.Width * 0.5f - edgeMargin);

        Physics.SyncTransforms();

        if (!TryFindFreeOffset(point, limit, out float firstOffset))
        {
            return false;
        }

        PlaceFuelTank(point, firstOffset);

        if (Random.value < doubleRowChance)
        {
            PlaceFuelTank(point, GetSecondOffset(firstOffset, limit));
        }

        return true;
    }

    private bool TryFindFreeOffset(RoadPathPoint point, float limit, out float offset)
    {
        for (int attempt = 0; attempt < placementAttempts; attempt++)
        {
            offset = Random.Range(-limit, limit);

            if (IsClear(point.Position + point.Right * offset))
            {
                return true;
            }
        }

        offset = 0f;
        return false;
    }

    private bool IsClear(Vector3 position)
    {
        int count = Physics.OverlapSphereNonAlloc(
            position,
            clearanceRadius,
            overlapBuffer,
            ~0,
            QueryTriggerInteraction.Collide);

        for (int i = 0; i < count; i++)
        {
            Collider hit = overlapBuffer[i];

            if (hit.GetComponentInParent<ObstacleSpawner>() != null)
            {
                return false;
            }

            if (hit.GetComponentInParent<FuelTankSpawner>() != null)
            {
                return false;
            }
        }

        return true;
    }

    private void PlaceFuelTank(RoadPathPoint point, float lateralOffset)
    {
        FuelTank FuelTank = pool.Get();
        FuelTank.Transform.rotation = Quaternion.LookRotation(point.Forward, point.Up);

        float halfHeight = FuelTank.Renderer.bounds.extents.y;
        float bob = Mathf.Min(bobHeight, halfHeight * 0.5f);

        FuelTank.Transform.position = point.Position + point.Right * lateralOffset + point.Up * (halfHeight + bob);

        FuelTank.Motion.Begin(bob, bobSpeed, flipSpeed);

        activeFuelTanks.Add(FuelTank);
    }

    private float GetSecondOffset(float firstOffset, float limit)
    {
        float offset = Random.Range(-limit, limit);

        if (Mathf.Abs(offset - firstOffset) >= minRowSeparation)
        {
            return offset;
        }

        float direction = offset >= firstOffset ? 1f : -1f;
        float pushed = firstOffset + direction * minRowSeparation;

        if (Mathf.Abs(pushed) > limit)
        {
            pushed = firstOffset - direction * minRowSeparation;
        }

        return Mathf.Clamp(pushed, -limit, limit);
    }

    private bool TryGetPointAhead(out RoadPathPoint point)
    {
        if (!road.TryGetPathPoint(car.position, out point))
        {
            return false;
        }

        float travelled = 0f;

        while (travelled < spawnDistance)
        {
            float step = Mathf.Min(SampleStep, spawnDistance - travelled);

            if (!road.TryGetPathPoint(point.Position + point.Forward * step, out point))
            {
                return false;
            }

            travelled += step;
        }

        return true;
    }

    private void RecycleBehindCar()
    {
        for (int i = activeFuelTanks.Count - 1; i >= 0; i--)
        {
            FuelTank FuelTank = activeFuelTanks[i];

            if (Vector3.Dot(car.position - FuelTank.Transform.position, car.forward) > despawnDistance)
            {
                pool.Release(FuelTank);
                activeFuelTanks.RemoveAt(i);
            }
        }
    }

    private void LogError(string message, Object context = null)
    {
        Debug.LogError($"{nameof(FuelTankSpawner)}: {message}", context != null ? context : this);
    }

    private sealed class FuelTank
    {
        public FuelTank(GameObject gameObject)
        {
            GameObject = gameObject;
            Transform = gameObject.transform;
            Renderer = gameObject.GetComponentInChildren<Renderer>();
            Motion = gameObject.GetComponent<FuelTankMotion>();

            if (Motion == null)
            {
                Motion = gameObject.AddComponent<FuelTankMotion>();
            }
        }

        public GameObject GameObject { get; }
        public Transform Transform { get; }
        public Renderer Renderer { get; }
        public FuelTankMotion Motion { get; }
    }
}