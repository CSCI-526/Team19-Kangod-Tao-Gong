using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public sealed class AutoDriveCar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InfiniteRoad road;
    [SerializeField] private Transform visual;

    [Header("Driving")]
    [SerializeField, Min(0f)] private float forwardSpeed = 15f;
    [SerializeField, Min(0.01f)] private float maxLateralSpeed = 8f;
    [SerializeField, Min(0.01f)] private float lateralAcceleration = 30f;
    [SerializeField, Min(0f)] private float edgeMargin = 1f;

    [Header("Feel")]
    [SerializeField, Range(0f, 45f)] private float maxSteerAngle = 20f;
    [SerializeField, Min(0f)] private float turnSharpness = 12f;

    private InputAction steerAction;
    private float lateralOffset;
    private float lateralSpeed;
    private float heightOffset;

    public float SteerInput { get; private set; }

    private void Awake()
    {
        steerAction = CreateSteerAction();
    }

    private void OnEnable()
    {
        steerAction.Enable();
    }

    private void OnDisable()
    {
        steerAction.Disable();
    }

    private void OnDestroy()
    {
        steerAction.Dispose();
    }

    private void Start()
    {
        if (road == null || !road.TryGetPathPoint(transform.position, out RoadPathPoint start))
        {
            Debug.LogError($"{nameof(AutoDriveCar)}: needs a road reference, and the car must start on the road.", this);
            enabled = false;
            return;
        }

        Vector3 offset = transform.position - start.Position;
        lateralOffset = Vector3.Dot(offset, start.Right);
        heightOffset = Vector3.Dot(offset, start.Up);
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        float steerInput = Mathf.Clamp(steerAction.ReadValue<float>(), -1f, 1f);
        SteerInput = steerInput;

        lateralSpeed = Mathf.MoveTowards(lateralSpeed, steerInput * maxLateralSpeed, lateralAcceleration * deltaTime);

        if (!road.TryGetPathPoint(transform.position, out RoadPathPoint current))
        {
            return;
        }

        Vector3 lookAhead = current.Position + current.Forward * (forwardSpeed * deltaTime);

        if (!road.TryGetPathPoint(lookAhead, out RoadPathPoint next))
        {
            return;
        }

        float limit = Mathf.Max(0f, next.Width * 0.5f - edgeMargin);
        float desiredOffset = lateralOffset + lateralSpeed * deltaTime;

        lateralOffset = Mathf.Clamp(desiredOffset, -limit, limit);

        if (!Mathf.Approximately(desiredOffset, lateralOffset))
        {
            lateralSpeed = 0f;
        }

        transform.position = next.Position + next.Right * lateralOffset + next.Up * heightOffset;
        ApplyRotation(next, deltaTime);
    }

    private void ApplyRotation(RoadPathPoint point, float deltaTime)
    {
        float blend = 1f - Mathf.Exp(-turnSharpness * deltaTime);
        float steerAngle = maxSteerAngle * lateralSpeed / maxLateralSpeed;
        Quaternion roadRotation = Quaternion.LookRotation(point.Forward, point.Up);

        if (visual != null)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, roadRotation, blend);
            visual.localRotation = Quaternion.Slerp(visual.localRotation, Quaternion.Euler(0f, steerAngle, 0f), blend);
            return;
        }

        Quaternion target = roadRotation * Quaternion.Euler(0f, steerAngle, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, blend);
    }

    private static InputAction CreateSteerAction()
    {
        InputAction action = new InputAction("Steer", InputActionType.Value, expectedControlType: "Axis");

        action.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/leftArrow")
            .With("Positive", "<Keyboard>/rightArrow");

        action.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/a")
            .With("Positive", "<Keyboard>/d");


        return action;
    }
}