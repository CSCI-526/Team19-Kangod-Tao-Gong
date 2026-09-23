using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class FuelBar : MonoBehaviour
{
    private const float Margin = 24f;
    private const float Width = 220f;
    private const float Height = 20f;
    private const float Border = 2f;
    private const float LabelWidth = 64f;
    private const float LabelGap = 8f;
    private const float LabelSize = 18f;

    private static readonly Color BackgroundColor = new Color(0f, 0f, 0f, 0.55f);
    private static readonly Color FullColor = new Color(0.25f, 0.85f, 0.35f);
    private static readonly Color EmptyColor = new Color(0.9f, 0.2f, 0.15f);

    private Image fill;

    public static FuelBar Create(TMP_FontAsset font)
    {
        GameObject root = new GameObject("FuelBar", typeof(Canvas), typeof(CanvasScaler), typeof(FuelBar));

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        FuelBar bar = root.GetComponent<FuelBar>();
        bar.Build(root.transform, font);

        return bar;
    }

    public void SetFill(float normalized)
    {
        normalized = Mathf.Clamp01(normalized);

        fill.fillAmount = normalized;
        fill.color = Color.Lerp(EmptyColor, FullColor, normalized);
    }

    private void Build(Transform parent, TMP_FontAsset font)
    {
        CreateLabel(parent, font);

        RectTransform background = CreateImage("Background", parent, BackgroundColor).rectTransform;
        background.anchorMin = Vector2.one;
        background.anchorMax = Vector2.one;
        background.pivot = Vector2.one;
        background.anchoredPosition = new Vector2(-Margin, -Margin);
        background.sizeDelta = new Vector2(Width, Height);

        fill = CreateImage("Fill", background, FullColor);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;

        RectTransform fillRect = fill.rectTransform;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(Border, Border);
        fillRect.offsetMax = new Vector2(-Border, -Border);
    }

    private static void CreateLabel(Transform parent, TMP_FontAsset font)
    {
        GameObject node = new GameObject("Label", typeof(RectTransform));
        node.transform.SetParent(parent, false);

        TextMeshProUGUI text = node.AddComponent<TextMeshProUGUI>();

        if (font != null)
        {
            text.font = font;
        }

        text.text = "FUEL";
        text.fontSize = LabelSize;
        text.alignment = TextAlignmentOptions.MidlineRight;
        text.color = Color.white;
        text.raycastTarget = false;

        RectTransform rect = text.rectTransform;
        rect.anchorMin = Vector2.one;
        rect.anchorMax = Vector2.one;
        rect.pivot = Vector2.one;
        rect.anchoredPosition = new Vector2(-(Margin + Width + LabelGap), -Margin);
        rect.sizeDelta = new Vector2(LabelWidth, Height);
    }

    private static Image CreateImage(string name, Transform parent, Color color)
    {
        GameObject node = new GameObject(name, typeof(RectTransform), typeof(Image));
        node.transform.SetParent(parent, false);

        Image image = node.GetComponent<Image>();
        image.sprite = WhiteSprite();
        image.color = color;
        image.raycastTarget = false;

        return image;
    }

    private static Sprite cachedSprite;

    private static Sprite WhiteSprite()
    {
        if (cachedSprite != null)
        {
            return cachedSprite;
        }

        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        cachedSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));

        return cachedSprite;
    }
}
