using UnityEngine;

[DisallowMultipleComponent]
public sealed class ChaseMeter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private ChaseCar[] chaseCars;

    private AutoDriveCar playerCar;
    private HandleCrash crashHandler;

    [Header("Steering Condition")]
    [SerializeField, Range(0f, 1f)] private float steerThreshold = 0.15f;
    [SerializeField, Min(0f)] private float steerGraceTime = 2f;
    [SerializeField, Min(0f)] private float steerDrainRate = 0.1f;

    [Header("Chase Proximity Condition")]
    [SerializeField, Min(0f)] private float dangerDistance = 5f;
    [SerializeField, Min(0f)] private float safeDistance = 20f;
    [SerializeField, Min(0f)] private float proximityDrainRate = 0.15f;

    private float idleSteerTime;

    public float Value { get; private set; } = 1f;

    private void Awake()
    {
        if (player == null)
        {
            Debug.LogError($"{nameof(ChaseMeter)}: player reference is missing.", this);
            enabled = false;
            return;
        }

        playerCar = player.GetComponent<AutoDriveCar>();
        crashHandler = player.GetComponent<HandleCrash>();

        if (playerCar == null || crashHandler == null)
        {
            Debug.LogError($"{nameof(ChaseMeter)}: player needs both Auto Drive Car and Handle Crash.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        Value = Mathf.Clamp01(Value - GetTotalDrainRate() * Time.deltaTime);

        if (Value <= 0f)
        {
            crashHandler.Crash();
        }
    }

    private float GetTotalDrainRate()
    {
        return GetSteeringDrain() + GetProximityDrain();
    }

    private float GetSteeringDrain()
    {
        if (Mathf.Abs(playerCar.SteerInput) < steerThreshold)
        {
            idleSteerTime += Time.deltaTime;
        }
        else
        {
            idleSteerTime = 0f;
        }

        return idleSteerTime >= steerGraceTime ? steerDrainRate : 0f;
    }

    private float GetProximityDrain()
    {
        if (chaseCars == null || chaseCars.Length == 0)
        {
            return 0f;
        }

        float closest = float.MaxValue;

        foreach (ChaseCar chaseCar in chaseCars)
        {
            if (chaseCar != null && chaseCar.DistanceToPlayer < closest)
            {
                closest = chaseCar.DistanceToPlayer;
            }
        }

        float danger = 1f - Mathf.InverseLerp(dangerDistance, safeDistance, closest);
        return danger * proximityDrainRate;
    }
}