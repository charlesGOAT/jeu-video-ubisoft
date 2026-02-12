using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStatsUI : MonoBehaviour
{
    private static GameStatsUI _instance;

    public float gameDuration = 180f;
    public float barLerpSpeed = 10f;
    public Vector2 referenceResolution = new Vector2(1920f, 1080f);
    public float hudHeight = 128f;
    public float sidePadding = 24f;
    public float centerGap = 240f;
    public float barHeight = 34f;
    public int fontSize = 30;
    public int smallFontSize = 18;
    public float shineSpeed = 520f;
    public float pulseStrength = 0.06f;
    public float pulseDuration = 0.14f;

    private float _remaining;
    private GridManagerStategy _gridManager;
    private Canvas _canvas;

    private Text _timerText;
    private Text _leftLabel;
    private Text _rightLabel;
    private Text _leftSub;
    private Text _rightSub;
    private Text _leftValue;
    private Text _rightValue;

    private Image _leftFill;
    private Image _rightFill;

    private RectTransform _leftPlateRect;
    private RectTransform _rightPlateRect;
    private RectTransform _leftShineRect;
    private RectTransform _rightShineRect;

    private float _leftPercent;
    private float _rightPercent;
    private int _lastP1;
    private int _lastP2;
    private float _leftPulse;
    private float _rightPulse;
    private Font _font;
    private Sprite _uiSprite;
    private Sprite _leftInkSprite;
    private Sprite _rightInkSprite;
    private Sprite _shineSprite;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        _remaining = gameDuration;
        SetupUI();
        _gridManager = FindFirstObjectByType<GridManagerStategy>();
        if (_gridManager == null)
        {
            Debug.LogWarning("GameStatsUI: no GridManagerStategy found in scene; tile counts will be unavailable until one is present.");
        }
    }

    private void SetupUI()
    {
        var canvasGO = new GameObject("GameStatsCanvas");
        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasGO);

        _font = ResolveFont();
        _uiSprite = ResolveUISprite();
        _shineSprite = CreateShineSprite();

        var hudGO = new GameObject("HUD");
        hudGO.transform.SetParent(canvasGO.transform, false);
        var hudRect = hudGO.AddComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0f, 1f);
        hudRect.anchorMax = new Vector2(1f, 1f);
        hudRect.pivot = new Vector2(0.5f, 1f);
        hudRect.anchoredPosition = Vector2.zero;
        hudRect.sizeDelta = new Vector2(0f, hudHeight);

        var hudBg = hudGO.AddComponent<Image>();
        hudBg.sprite = _uiSprite;
        hudBg.type = Image.Type.Sliced;
        hudBg.color = new Color(0.06f, 0.10f, 0.16f, 0.82f);
        var hudOutline = hudGO.AddComponent<Outline>();
        hudOutline.effectColor = new Color(0f, 0f, 0f, 0.55f);
        hudOutline.effectDistance = new Vector2(2f, -2f);

        var leftBar = BuildSide(hudGO.transform, isLeft: true);
        var rightBar = BuildSide(hudGO.transform, isLeft: false);

        _leftFill = leftBar.fill;
        _rightFill = rightBar.fill;
        _leftPlateRect = leftBar.plate;
        _rightPlateRect = rightBar.plate;
        _leftShineRect = leftBar.shine;
        _rightShineRect = rightBar.shine;
        _leftLabel = leftBar.label;
        _rightLabel = rightBar.label;
        _leftSub = leftBar.sub;
        _rightSub = rightBar.sub;
        _leftValue = leftBar.value;
        _rightValue = rightBar.value;

        var timerPlateGO = new GameObject("TimerPlate");
        timerPlateGO.transform.SetParent(hudGO.transform, false);
        var timerPlateRect = timerPlateGO.AddComponent<RectTransform>();
        timerPlateRect.anchorMin = new Vector2(0.5f, 0.5f);
        timerPlateRect.anchorMax = new Vector2(0.5f, 0.5f);
        timerPlateRect.pivot = new Vector2(0.5f, 0.5f);
        timerPlateRect.anchoredPosition = new Vector2(0f, -2f);
        timerPlateRect.sizeDelta = new Vector2(centerGap - 28f, 54f);

        var timerPlate = timerPlateGO.AddComponent<Image>();
        timerPlate.sprite = _uiSprite;
        timerPlate.type = Image.Type.Sliced;
        timerPlate.color = new Color(0f, 0f, 0f, 0.65f);
        var timerOutline = timerPlateGO.AddComponent<Outline>();
        timerOutline.effectColor = new Color(0f, 0f, 0f, 0.6f);
        timerOutline.effectDistance = new Vector2(2f, -2f);

        var timerTextGO = new GameObject("TimerText");
        timerTextGO.transform.SetParent(timerPlateGO.transform, false);
        _timerText = timerTextGO.AddComponent<Text>();
        _timerText.font = _font;
        _timerText.fontSize = fontSize + 6;
        _timerText.alignment = TextAnchor.MiddleCenter;
        _timerText.color = Color.white;
        var timerTextRect = timerTextGO.GetComponent<RectTransform>();
        timerTextRect.anchorMin = new Vector2(0f, 0f);
        timerTextRect.anchorMax = new Vector2(1f, 1f);
        timerTextRect.offsetMin = Vector2.zero;
        timerTextRect.offsetMax = Vector2.zero;

        SetSideVisuals();
    }

    private void Update()
    {
        if (_remaining > 0f)
        {
            _remaining -= Time.deltaTime;
            if (_remaining < 0f) _remaining = 0f;
        }

        UpdateUI();
        UpdateFx();
    }

    private void UpdateUI()
    {
        int p1 = 0;
        int p2 = 0;
        int total = 0;

        if (_gridManager != null)
        {
            p1 = _gridManager.tilesPerPlayer[0];
            p2 = _gridManager.tilesPerPlayer[1];
            if (_gridManager.Width > 0 && _gridManager.Height > 0)
                total = _gridManager.Width * _gridManager.Height;
            else
                total = Mathf.Max(1, p1 + p2);
        }
        else
        {
            total = 1;
        }

        float targetRight = Mathf.Clamp01(p1 / (float)Mathf.Max(1, total));
        float targetLeft = Mathf.Clamp01(p2 / (float)Mathf.Max(1, total));

        if (p1 != _lastP1)
            _rightPulse = pulseDuration;
        if (p2 != _lastP2)
            _leftPulse = pulseDuration;
        _lastP1 = p1;
        _lastP2 = p2;

        _rightPercent = Mathf.Lerp(_rightPercent, targetRight, Time.deltaTime * barLerpSpeed);
        _leftPercent = Mathf.Lerp(_leftPercent, targetLeft, Time.deltaTime * barLerpSpeed);

        if (_rightFill != null) _rightFill.fillAmount = _rightPercent;
        if (_leftFill != null) _leftFill.fillAmount = _leftPercent;

        if (_rightValue != null) _rightValue.text = p1 + "/" + total;
        if (_leftValue != null) _leftValue.text = p2 + "/" + total;

        var stats = GameStatsManager.Instance;
        if (_rightSub != null) _rightSub.text = "BOMB " + stats.BombsPlaced[0] + "  INK " + stats.TilesPainted[0] + "  STEAL " + stats.TilesStolen[0];
        if (_leftSub != null) _leftSub.text = "BOMB " + stats.BombsPlaced[1] + "  INK " + stats.TilesPainted[1] + "  STEAL " + stats.TilesStolen[1];

        TimeSpan ts = TimeSpan.FromSeconds(_remaining);
        _timerText.text = string.Format("{0:D2}:{1:D2}", ts.Minutes, ts.Seconds);
    }

    private void UpdateFx()
    {
        float dt = Time.deltaTime;

        if (_leftPulse > 0f) _leftPulse -= dt;
        if (_rightPulse > 0f) _rightPulse -= dt;

        if (_leftPlateRect != null)
        {
            float t = Mathf.Clamp01(_leftPulse / Mathf.Max(0.001f, pulseDuration));
            float s = 1f + (Mathf.Sin((1f - t) * Mathf.PI) * pulseStrength);
            _leftPlateRect.localScale = new Vector3(s, s, 1f);
        }

        if (_rightPlateRect != null)
        {
            float t = Mathf.Clamp01(_rightPulse / Mathf.Max(0.001f, pulseDuration));
            float s = 1f + (Mathf.Sin((1f - t) * Mathf.PI) * pulseStrength);
            _rightPlateRect.localScale = new Vector3(s, s, 1f);
        }

        if (_leftShineRect != null)
        {
            float w = _leftShineRect.parent.GetComponent<RectTransform>().rect.width;
            float x = Mathf.Repeat(Time.time * shineSpeed, w + 260f) - 260f;
            _leftShineRect.anchoredPosition = new Vector2(x, 0f);
        }

        if (_rightShineRect != null)
        {
            float w = _rightShineRect.parent.GetComponent<RectTransform>().rect.width;
            float x = Mathf.Repeat(Time.time * shineSpeed, w + 260f) - 260f;
            _rightShineRect.anchoredPosition = new Vector2(-x, 0f);
        }
    }

    private void SetSideVisuals()
    {
        Color p1 = Color.white;
        Color p2 = Color.white;
        if (Player.PlayerColorDict.TryGetValue(PlayerEnum.Player1, out Color c1)) p1 = c1;
        if (Player.PlayerColorDict.TryGetValue(PlayerEnum.Player2, out Color c2)) p2 = c2;

        _rightLabel.text = "PLAYER 1";
        _leftLabel.text = "PLAYER 2";
        _rightLabel.color = p1;
        _leftLabel.color = p2;

        _rightInkSprite = CreateInkSprite(p1, 11);
        _leftInkSprite = CreateInkSprite(p2, 22);
        if (_rightFill != null) _rightFill.sprite = _rightInkSprite;
        if (_leftFill != null) _leftFill.sprite = _leftInkSprite;
    }

    private Font ResolveFont()
    {
        Font font = null;
        try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { font = null; }
        if (font != null) return font;
        try { font = Font.CreateDynamicFontFromOSFont("Arial", 16); } catch { font = null; }
        if (font != null) return font;
        try { font = Font.CreateDynamicFontFromOSFont(new string[] { "Arial", "Segoe UI", "Helvetica" }, 16); } catch { font = null; }
        if (font != null) return font;
        var all = Resources.FindObjectsOfTypeAll<Font>();
        if (all != null && all.Length > 0) return all[0];
        return null;
    }

    private Sprite ResolveUISprite()
    {
        int w = 64;
        int h = 64;
        int r = 14;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float ax = Mathf.Min(x, w - 1 - x);
                float ay = Mathf.Min(y, h - 1 - y);
                float a = 1f;
                if (ax < r && ay < r)
                {
                    float dx = r - ax;
                    float dy = r - ay;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    a = Mathf.Clamp01(1f - (d - r));
                }

                float edge = (x < 2 || y < 2 || x > w - 3 || y > h - 3) ? 0.85f : 1f;
                tex.SetPixel(x, y, new Color(edge, edge, edge, a));
            }
        }
        tex.Apply();

        var border = new Vector4(18f, 18f, 18f, 18f);
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
    }

    private Sprite CreateShineSprite()
    {
        int w = 128;
        int h = 16;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float u = x / (float)(w - 1);
                float v = y / (float)(h - 1);
                float diag = Mathf.Abs((u * 1.15f) - v);
                float a = Mathf.SmoothStep(0.18f, 0f, diag);
                a *= 0.20f;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0f, 0.5f), 100f);
    }

    private Sprite CreateInkSprite(Color baseColor, int seed)
    {
        int w = 128;
        int h = 16;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Point;

        float r = Mathf.Clamp01(baseColor.r);
        float g = Mathf.Clamp01(baseColor.g);
        float b = Mathf.Clamp01(baseColor.b);

        for (int y = 0; y < h; y++)
        {
            float v = y / (float)(h - 1);
            float shade = Mathf.Lerp(0.85f, 1.15f, v);
            for (int x = 0; x < w; x++)
            {
                float n = Mathf.PerlinNoise((x + seed) * 0.08f, (y + seed) * 0.25f);
                float a = Mathf.SmoothStep(0.75f, 1f, n);
                float ink = Mathf.Lerp(0.75f, 1.25f, n);
                var col = new Color(r * shade * ink, g * shade * ink, b * shade * ink, Mathf.Lerp(0.85f, 1f, a));
                tex.SetPixel(x, y, col);
            }
        }
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0f, 0.5f), 100f);
    }

    private (Image fill, Text label, Text sub, Text value, RectTransform plate, RectTransform shine) BuildSide(Transform parent, bool isLeft)
    {
        var plateGO = new GameObject(isLeft ? "LeftPlate" : "RightPlate");
        plateGO.transform.SetParent(parent, false);
        var plateRect = plateGO.AddComponent<RectTransform>();
        plateRect.anchorMin = isLeft ? new Vector2(0f, 0f) : new Vector2(0.5f, 0f);
        plateRect.anchorMax = isLeft ? new Vector2(0.5f, 1f) : new Vector2(1f, 1f);
        plateRect.pivot = isLeft ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);
        plateRect.offsetMin = new Vector2(isLeft ? sidePadding : centerGap * 0.5f, 10f);
        plateRect.offsetMax = new Vector2(isLeft ? -centerGap * 0.5f : -sidePadding, -12f);

        var plate = plateGO.AddComponent<Image>();
        plate.sprite = _uiSprite;
        plate.type = Image.Type.Sliced;
        plate.color = new Color(0f, 0f, 0f, 0.18f);

        var barBgGO = new GameObject(isLeft ? "LeftTerritory" : "RightTerritory");
        barBgGO.transform.SetParent(plateGO.transform, false);
        var barBgRect = barBgGO.AddComponent<RectTransform>();
        barBgRect.anchorMin = new Vector2(0f, 0.2f);
        barBgRect.anchorMax = new Vector2(1f, 0.2f);
        barBgRect.pivot = new Vector2(0.5f, 0.5f);
        barBgRect.anchoredPosition = new Vector2(0f, 0f);
        barBgRect.sizeDelta = new Vector2(0f, barHeight);

        var barBg = barBgGO.AddComponent<Image>();
        barBg.sprite = _uiSprite;
        barBg.type = Image.Type.Sliced;
        barBg.color = new Color(0f, 0f, 0f, 0.55f);
        var barOutline = barBgGO.AddComponent<Outline>();
        barOutline.effectColor = new Color(0f, 0f, 0f, 0.55f);
        barOutline.effectDistance = new Vector2(2f, -2f);

        var fillGO = new GameObject(isLeft ? "LeftFill" : "RightFill");
        fillGO.transform.SetParent(barBgGO.transform, false);
        var fillRect = fillGO.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = new Vector2(3f, 3f);
        fillRect.offsetMax = new Vector2(-3f, -3f);

        var fill = fillGO.AddComponent<Image>();
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = isLeft ? 0 : 1;
        fill.fillAmount = 0f;
        fill.color = Color.white;
            var shineGO = new GameObject(isLeft ? "LeftShine" : "RightShine");
            shineGO.transform.SetParent(barBgGO.transform, false);
            var shineRect = shineGO.AddComponent<RectTransform>();
            shineRect.anchorMin = new Vector2(0f, 0f);
            shineRect.anchorMax = new Vector2(0f, 1f);
            shineRect.pivot = new Vector2(0f, 0.5f);
            shineRect.anchoredPosition = new Vector2(-260f, 0f);
            shineRect.sizeDelta = new Vector2(260f, 0f);
            var shineImg = shineGO.AddComponent<Image>();
            shineImg.sprite = _shineSprite;
            shineImg.color = new Color(1f, 1f, 1f, 0.65f);
            var shineMask = barBgGO.AddComponent<Mask>();
            shineMask.showMaskGraphic = true;

        var fillShadow = fillGO.AddComponent<Shadow>();
        fillShadow.effectColor = new Color(0f, 0f, 0f, 0.2f);
        fillShadow.effectDistance = new Vector2(2f, -2f);

        var labelGO = new GameObject(isLeft ? "LeftLabel" : "RightLabel");
        labelGO.transform.SetParent(plateGO.transform, false);
        var labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0.68f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.offsetMin = new Vector2(10f, 0f);
        labelRect.offsetMax = new Vector2(-10f, 0f);
        var label = labelGO.AddComponent<Text>();
        label.font = _font;
        label.fontSize = fontSize;
        label.alignment = isLeft ? TextAnchor.UpperLeft : TextAnchor.UpperRight;
        label.color = Color.white;
        label.text = isLeft ? "PLAYER 2" : "PLAYER 1";

        var subGO = new GameObject(isLeft ? "LeftSub" : "RightSub");
        subGO.transform.SetParent(plateGO.transform, false);
        var subRect = subGO.AddComponent<RectTransform>();
        subRect.anchorMin = new Vector2(0f, 0.42f);
        subRect.anchorMax = new Vector2(1f, 0.68f);
        subRect.offsetMin = new Vector2(10f, 0f);
        subRect.offsetMax = new Vector2(-10f, 0f);
        var sub = subGO.AddComponent<Text>();
        sub.font = _font;
        sub.fontSize = smallFontSize;
        sub.alignment = isLeft ? TextAnchor.UpperLeft : TextAnchor.UpperRight;
        sub.color = new Color(1f, 1f, 1f, 0.85f);
        sub.text = "BOMB 0  INK 0  STEAL 0";

        var valueGO = new GameObject(isLeft ? "LeftValue" : "RightValue");
        valueGO.transform.SetParent(barBgGO.transform, false);
        var valueRect = valueGO.AddComponent<RectTransform>();
        valueRect.anchorMin = new Vector2(0f, 0f);
        valueRect.anchorMax = new Vector2(1f, 1f);
        valueRect.offsetMin = new Vector2(10f, 0f);
        valueRect.offsetMax = new Vector2(-10f, 0f);
        var value = valueGO.AddComponent<Text>();
        value.font = _font;
        value.fontSize = smallFontSize;
        value.alignment = isLeft ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
        value.color = Color.white;
        value.text = "0/0";

        return (fill, label, sub, value, plateRect, shineRect);
    }
}
