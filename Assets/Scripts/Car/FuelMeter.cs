using UnityEngine;

public sealed class FuelMeter : MonoBehaviour
{
    [SerializeField, Min(0)] private float maxFuel = 100f;
    [SerializeField, Min(0)] private float consumptionPerSecond = 4f;

    private float remainingFuel;
    private HandleCrash crash;

    private void Awake()
    {
        remainingFuel = maxFuel;
        crash = GetComponent<HandleCrash>();

    }


    private void Update()
    {
        int remainingFuelInt = Mathf.FloorToInt(remainingFuel);
        remainingFuel = Mathf.Max(0, remainingFuel - consumptionPerSecond * Time.deltaTime);
        int newRemainingFuelInt = Mathf.FloorToInt(remainingFuel);
        if (remainingFuelInt != newRemainingFuelInt)
        {
            Debug.Log($"Remaining Fuel: {newRemainingFuelInt}");
        }
        if (remainingFuel <= 0)
        {
            enabled = false;
            crash.Crash();
        }
    }
}