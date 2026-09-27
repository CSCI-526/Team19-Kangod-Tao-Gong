using UnityEngine;

public sealed class PickupHUD : MonoBehaviour
{
    private const float PopupDuration = 2f;

    [SerializeField] private CarPickupEffects effects;
    [SerializeField] private bool showControls = true;
    private GUIStyle body;
    private GUIStyle popupText;
    private GUIStyle popupDetail;
    private string popupMessage;
    private string popupHint;
    private float popupRemaining;
    private int shownPickupCount;

    private void Update()
    {
        if (effects == null) return;

        if (effects.PickupCount != shownPickupCount)
        {
            shownPickupCount = effects.PickupCount;
            bool reverse = effects.ActiveEffect == PickupEffectType.ReverseSteering;
            bool fullFuel = effects.LastOutcome == RandomPickupOutcome.FullFuel;
            popupMessage = fullFuel ? "FUEL FULL!" : reverse ? "CONTROLS REVERSED!" : null;
            popupHint = fullFuel ? "Fuel restored to 100%" : "A / Left: move right     D / Right: move left";
            popupRemaining = popupMessage != null && !effects.RunState.IsGameOver ? PopupDuration : 0f;
        }

        if (popupRemaining > 0f)
            popupRemaining = Mathf.Max(0f, popupRemaining - Time.unscaledDeltaTime);
    }

    private void OnDisable()
    {
        popupRemaining = 0f;
        shownPickupCount = 0;
    }

    private static void DrawPanel(Rect rect)
    {
        Color previousColor = GUI.color;
        GUI.color = new Color(0.03f, 0.04f, 0.06f, 0.9f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previousColor;
    }

    private void OnGUI()
    {
        if (effects == null) return;
        if (body == null)
            body = new GUIStyle(GUI.skin.label) { fontSize = 18, wordWrap = true, normal = { textColor = Color.white } };
        if (popupText == null)
        {
            popupText = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 30,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = Color.white }
            };
            popupDetail = new GUIStyle(body) { alignment = TextAnchor.MiddleCenter };
        }

        float scale = Mathf.Clamp(Screen.width / 1280f, 0.65f, 1.5f);
        Matrix4x4 previous = GUI.matrix;
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
        float width = Screen.width / scale;
        float height = Screen.height / scale;
        Rect textArea = new Rect(32f, 32f, 340f, 150f);
        DrawPanel(new Rect(20f, 20f, 364f, 166f));
        RiskRunState run = effects.RunState;

        string title = effects.ActiveEffect == PickupEffectType.ReverseSteering
            ? $"REVERSED: {effects.RemainingSeconds:0.0}s"
            : "Controls: normal";

        GUI.Label(new Rect(textArea.x, textArea.y, textArea.width, 26f),
            $"Fuel {effects.CurrentFuel:0}/{FuelState.MaximumFuel:0}", body);
        GUI.Label(new Rect(textArea.x, textArea.y + 27f, textArea.width, 26f),
            $"Police gap: {run.PursuitGap:0.0} m", body);

        string pursuit = run.IsGameOver ? "CAUGHT! Press R to restart."
            : run.CrashSlowRemaining > 0f ? $"SLOWED {run.CrashSlowRemaining:0.0}s - STEER!"
            : run.RecoveryWaitRemaining > 0f ? "RECOVERING - KEEP DRIVING"
            : "Avoid crashes to escape";
        GUI.Label(new Rect(textArea.x, textArea.y + 54f, textArea.width, 28f), pursuit, body);

        GUI.Label(new Rect(textArea.x, textArea.y + 84f, textArea.width, 30f), title, body);
        string outcome = effects.LastOutcome == RandomPickupOutcome.FullFuel
            ? "Last pickup: full fuel"
            : effects.LastOutcome == RandomPickupOutcome.ReverseSteering
                ? "Last pickup: reverse"
                : "Pickup: mystery";
        GUI.Label(new Rect(textArea.x, textArea.y + 113f, textArea.width, 36f), outcome, body);

        if (showControls)
        {
            DrawPanel(new Rect(20f, height - 90f, width - 40f, 78f));
            GUI.Label(new Rect(32f, height - 84f, width - 64f, 32f),
                "A/D or Left/Right: steer. Avoid obstacles.", body);
            GUI.Label(new Rect(32f, height - 49f, width - 64f, 36f),
                "Pickup: 50% FUEL FULL or 50% REVERSE for 5s.", body);
        }

        if (popupRemaining > 0f || run.IsGameOver)
        {
            float popupWidth = Mathf.Min(620f, width - 80f);
            Rect popupRect = new Rect(
                (width - popupWidth) * 0.5f,
                height * 0.4f,
                popupWidth,
                100f
            );
            DrawPanel(popupRect);
            GUI.Label(new Rect(popupRect.x, popupRect.y + 4f, popupWidth, 52f),
                run.IsGameOver ? "CAUGHT!" : popupMessage, popupText);
            GUI.Label(new Rect(popupRect.x + 12f, popupRect.y + 56f, popupWidth - 24f, 40f),
                run.IsGameOver ? "Press R to restart" : popupHint, popupDetail);
        }

        GUI.matrix = previous;
    }
}
