using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public sealed class HandleCrash : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ObstacleSpawner obstacles;
    [SerializeField] private ChaseMeter meter;

    [Header("Obstacle Collision")]
    [SerializeField, Range(0f, 1f)]
    private float crashDrainAmount = 0.5f;

    private bool restarting;

    private void Awake()
    {
        Rigidbody body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;

        BoxCollider box = GetComponent<BoxCollider>();
        box.isTrigger = true;
    }

    private void Reset()
    {
        Rigidbody body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;

        BoxCollider box = GetComponent<BoxCollider>();
        box.isTrigger = true;

        Renderer visual =
            GetComponentInChildren<Renderer>();

        if (visual != null)
        {
            box.center =
                transform.InverseTransformPoint(
                    visual.bounds.center
                );

            box.size = visual.bounds.size;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (restarting)
        {
            return;
        }

        ChaseCar chaseCar =
            other.GetComponentInParent<ChaseCar>();

        if (chaseCar != null)
        {
            Crash();
            return;
        }

        if (obstacles == null ||
            !other.transform.IsChildOf(
                obstacles.transform
            ))
        {
            return;
        }

        if (meter != null)
        {
            meter.ApplyDrain(crashDrainAmount);
        }
    }

    public void Crash()
    {
        if (restarting)
        {
            return;
        }

        restarting = true;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}