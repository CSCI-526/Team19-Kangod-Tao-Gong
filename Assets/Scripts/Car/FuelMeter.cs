using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class FuelMeter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_FontAsset font;

    [Header("Fuel")]
    [SerializeField, Min(0.01f)] private float maxFuel = 100f;
    [SerializeField, Min(0f)] private float consumptionPerSecond = 4f;

    private HandleCrash crash;
    private FuelBar bar;

    public float Fuel { get; private set; }
    public float Normalized => Fuel / maxFuel;

    private void Awake()
    {
        crash = GetComponentInParent<HandleCrash>();

        if (crash == null)
        {
            crash = FindFirstObjectByType<HandleCrash>();
        }

        if (crash == null)
        {
            Debug.LogError($"{nameof(FuelMeter)}: no {nameof(HandleCrash)} found in the scene.", this);
            enabled = false;
            return;
        }

        bar = FuelBar.Create(font);

        Fuel = maxFuel;
        bar.SetFill(1f);
    }

    private void Update()
    {
        Fuel = Mathf.Max(0f, Fuel - consumptionPerSecond * Time.deltaTime);
        bar.SetFill(Normalized);

        if (Fuel > 0f)
        {
            return;
        }

        enabled = false;
        crash.Crash();
    }

    public void Refuel(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        Fuel = Mathf.Min(Fuel + amount, maxFuel);
        bar.SetFill(Normalized);
    }
}
