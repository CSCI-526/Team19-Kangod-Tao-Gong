using UnityEngine;

public sealed class PickupHUD : MonoBehaviour
{
    [SerializeField] private CarPickupEffects effects;
    [SerializeField] private bool showControls = true;
    private GUIStyle body;

    private void OnGUI()
    {
        if (effects == null) return;
        if (body == null)
            body = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true };

        float scale = Mathf.Clamp(Screen.width / 1280f, 0.65f, 1.5f);
        Matrix4x4 previous = GUI.matrix;
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
        float width = Screen.width / scale;
        float height = Screen.height / scale;
        Rect textArea = new Rect(width - 500f, 16f, 480f, 156f);
        RiskRunState run = effects.RunState;

        string title = effects.ActiveEffect == PickupEffectType.ReverseSteering
            ? $"Effect: reversed controls ({effects.RemainingSeconds:0.0}s left)"
            : "Effect: none";

        GUI.Label(new Rect(textArea.x, textArea.y, textArea.width, 26f),
            $"Fuel: {effects.CurrentFuel:0}/{FuelState.MaximumFuel:0}", body);
        GUI.Label(new Rect(textArea.x, textArea.y + 27f, textArea.width, 26f),
            $"Police distance: {run.PursuitGap:0.0} m", body);

        string pursuit = run.IsGameOver ? "CAUGHT! Press R to restart."
            : run.CrashSlowRemaining > 0f ? $"Crash: slowed for {run.CrashSlowRemaining:0.0}s. Keep steering!"
            : run.RecoveryWaitRemaining > 0f ? "Recovering from the crash. Keep driving!"
            : "Clean driving pulls you away from the police.";
        GUI.Label(new Rect(textArea.x, textArea.y + 54f, textArea.width, 28f), pursuit, body);

        GUI.Label(new Rect(textArea.x, textArea.y + 84f, textArea.width, 30f), title, body);
        string outcome = effects.LastOutcome == RandomPickupOutcome.FullFuel
            ? "Last pickup: fuel refilled."
            : effects.LastOutcome == RandomPickupOutcome.ReverseSteering
                ? "Last pickup: controls reversed."
                : "Pickup result is hidden until collection.";
        GUI.Label(new Rect(textArea.x, textArea.y + 113f, textArea.width, 36f), outcome, body);

        if (showControls)
        {
            GUI.Label(new Rect(20f, height - 77f, width - 40f, 28f),
                "Steer: A / D or Left / Right  |  Avoid obstacles and keep the police behind you. A crash slows the car; 0 m means caught.", body);
            GUI.Label(new Rect(20f, height - 47f, width - 40f, 36f),
                "Special pickup: 50% full fuel, 50% reversed controls for 5 seconds. The result is hidden until collection.", body);
        }

        GUI.matrix = previous;
    }
}
