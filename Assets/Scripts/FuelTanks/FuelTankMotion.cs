using UnityEngine;

[DisallowMultipleComponent]
public sealed class FuelTankMotion : MonoBehaviour
{
    private float bobHeight;
    private float bobSpeed;
    private float flipSpeed;

    private Vector3 basePosition;
    private Quaternion baseRotation;
    private Vector3 up;
    private float phase;
    private float angle;

    public void Begin(float bobHeight, float bobSpeed, float flipSpeed)
    {
        this.bobHeight = bobHeight;
        this.bobSpeed = bobSpeed;
        this.flipSpeed = flipSpeed;

        basePosition = transform.position;
        baseRotation = transform.rotation;
        up = baseRotation * Vector3.up;

        phase = Random.Range(0f, Mathf.PI * 2f);
        angle = Random.Range(0f, 360f);

        Apply();
    }

    private void Update()
    {
        phase += bobSpeed * Time.deltaTime;
        angle += flipSpeed * Time.deltaTime;

        Apply();
    }

    private void Apply()
    {
        transform.position = basePosition + up * (Mathf.Sin(phase) * bobHeight);
        transform.rotation = baseRotation * Quaternion.Euler(0f, Mathf.PingPong(angle, 180f), 0f);
    }
}
