using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public sealed class HandleCrash : MonoBehaviour
{
    [SerializeField] private ObstacleSpawner obstacles;
    [SerializeField, Min(0f)] private float fuelTankFuel = 40f;

    private FuelMeter fuel;

    private bool restarting;

    private void Reset()
    {
        Rigidbody body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;

        Renderer visual = GetComponentInChildren<Renderer>();

        if (visual != null)
        {
            BoxCollider box = GetComponent<BoxCollider>();
            box.center = transform.InverseTransformPoint(visual.bounds.center);
            box.size = visual.bounds.size;
        }

        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void Awake()
    {
        fuel = GetComponent<FuelMeter>();

        if (fuel == null)
        {
            fuel = FindFirstObjectByType<FuelMeter>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (restarting)
        {
            return;
        }

        FuelTankSpawner fuelTanks = other.GetComponentInParent<FuelTankSpawner>();

        if (fuelTanks != null)
        {
            CollectFuelTank(fuelTanks, other.transform);
            return;
        }

        if (obstacles != null && other.transform.IsChildOf(obstacles.transform))
        {
            Crash();
        }
    }

    private void CollectFuelTank(FuelTankSpawner fuelTanks, Transform hit)
    {
        if (!fuelTanks.TryCollect(hit))
        {
            return;
        }

        if (fuel != null)
        {
            fuel.Refuel(fuelTankFuel);
        }
    }

    public void Crash()
    {
        if (restarting)
        {
            return;
        }

        restarting = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}