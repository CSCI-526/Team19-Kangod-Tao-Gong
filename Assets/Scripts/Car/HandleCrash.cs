using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public sealed class HandleCrash : MonoBehaviour
{
    [SerializeField] private ObstacleSpawner obstacles;


    private bool restarting;
    private FuelMeter meter;

    private void Awake()
    {
        meter = GetComponent<FuelMeter>();
    }
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

    public void Crash()
    {
        if (restarting)
        {
            return;
        }
        restarting = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    private void OnTriggerEnter(Collider other)
    {

        FuelTank tank = other.GetComponent<FuelTank>();
        if (tank != null)
        {
            meter.ChangeFuel(tank.FuelAmount);
            Destroy(other.gameObject);
            return;
        }
        if (obstacles == null || !other.transform.IsChildOf(obstacles.transform))
        {
            return;
        }

        Crash();

    }
}