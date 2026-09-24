using UnityEngine;
using UnityEngine.UI;

public sealed class FuelMeter : MonoBehaviour
{
    [SerializeField, Min(0)] private float maxFuel = 100f;
    [SerializeField, Min(0)] private float consumptionPerSecond = 4f;

    [SerializeField] private Image bar;

    private float remainingFuel;
    private HandleCrash crash;

    private void Awake()
    {
        remainingFuel = maxFuel;
        crash = GetComponent<HandleCrash>();
        bar.fillAmount = remainingFuel / maxFuel;

    }


    private void Update()
    {
        remainingFuel = Mathf.Max(0, remainingFuel - consumptionPerSecond * Time.deltaTime);
        bar.fillAmount = remainingFuel / maxFuel;

        if (remainingFuel <= 0)
        {
            enabled = false;
            crash.Crash();
        }
    }

    public void ChangeFuel(float amount)
    {
        remainingFuel = Mathf.Clamp(remainingFuel + amount, 0f, maxFuel);
        bar.fillAmount = remainingFuel / maxFuel;
    }
}