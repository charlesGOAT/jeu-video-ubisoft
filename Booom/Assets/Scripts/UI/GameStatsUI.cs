using System;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class GameStatsUI : MonoBehaviour
{
    public float gameDuration = 180f;
    public float barLerpSpeed = 10f;
    public float shineSpeed = 520f;
    public Vector2 referenceResolution = new Vector2(1920f, 1080f);

    [SerializeField]
    private UIDocument document;

    private UIDocument _document;
    private GridManagerStategy _gridManager;
    private GameStatsManager _statsManager;

    private float _remaining;
    private float _leftPercent;
    private float _rightPercent;

    private VisualElement _leftFill;
    private VisualElement _rightFill;
    private VisualElement _leftShine;
    private VisualElement _rightShine;
    private VisualElement _leftBar;
    private VisualElement _rightBar;
    private VisualElement _leftPlate;
    private VisualElement _rightPlate;
    private VisualElement _leftAccent;
    private VisualElement _rightAccent;
    private VisualElement _centerPlate;
    private VisualElement _centerGlow;

    private Label _timerText;
    private Label _leftName;
    private Label _rightName;
    private Label _leftSub;
    private Label _rightSub;
    private Label _leftBombValue;
    private Label _leftInkValue;
    private Label _leftStealValue;
    private Label _rightBombValue;
    private Label _rightInkValue;
    private Label _rightStealValue;
    private Label _leftValue;
    private Label _rightValue;
    private Label _centerLeftPercent;
    private Label _centerRightPercent;
    private Label _leftLead;
    private Label _rightLead;
    private Label _centerDelta;

    private bool _loggedMissingDocument;
    private bool _loggedMissingPanelSettings;
    private bool _bound;

    private bool _lastP1Lead;
    private bool _lastP2Lead;
    private float _p1Pulse;
    private float _p2Pulse;

    private Font _runtimeFont;

    private Texture2D _p1Ink;
    private Texture2D _p2Ink;

    private void Awake()
    {
        TryInitializeAndBind();
        _remaining = gameDuration;
    }

    private void OnEnable()
    {
        TryInitializeAndBind();
    }

    private void TryInitializeAndBind()
    {
        if (_document == null)
            _document = document != null ? document : GetComponent<UIDocument>();

        if (_document != null && document == null)
            document = _document;

        if (_document == null)
        {
            _document = gameObject.AddComponent<UIDocument>();
            document = _document;
        }

        if (_document.panelSettings == null)
        {
            var panel = Resources.Load<PanelSettings>("UI/PanelSettings");
            if (panel != null)
                _document.panelSettings = panel;
            else
            {
                var runtimePanel = ScriptableObject.CreateInstance<PanelSettings>();
                try
                {
                    runtimePanel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
                    runtimePanel.referenceResolution = new Vector2Int(Mathf.RoundToInt(referenceResolution.x), Mathf.RoundToInt(referenceResolution.y));
                }
                catch { }
                _document.panelSettings = runtimePanel;
                if (!_loggedMissingPanelSettings)
                {
                    _loggedMissingPanelSettings = true;
                    Debug.LogWarning("GameStatsUI: no PanelSettings assigned; using a runtime-generated PanelSettings. Create one and assign it on the UIDocument (or put it at Resources/UI/PanelSettings) for consistent scaling/theme.");
                }
            }
        }

        EnsureDocumentAssets();
        Bind();
        if (_bound)
        {
            ApplyFontIfNeeded();
            ApplyPlayerVisuals();
        }
    }

    private void EnsureDocumentAssets()
    {
        if (_document == null)
            return;

        if (_document.visualTreeAsset == null)
        {
            var tree = Resources.Load<VisualTreeAsset>("UI/GameStatsHUD");
            if (tree != null)
                _document.visualTreeAsset = tree;
        }

        var style = Resources.Load<StyleSheet>("UI/GameStatsHUD");
        if (style != null)
        {
            var root = _document.rootVisualElement;
            if (root == null)
                return;
            if (!root.styleSheets.Contains(style))
                root.styleSheets.Add(style);
        }
    }

    private void Bind()
    {
        if (_document == null)
            return;
        var root = _document.rootVisualElement;
        if (root == null)
            return;
        _leftPlate = root.Q<VisualElement>("leftPlate");
        _rightPlate = root.Q<VisualElement>("rightPlate");
        _leftAccent = root.Q<VisualElement>("leftAccent");
        _rightAccent = root.Q<VisualElement>("rightAccent");
        _centerPlate = root.Q<VisualElement>("centerPlate");
        _centerGlow = root.Q<VisualElement>("centerGlow");
        _timerText = root.Q<Label>("timerText");
        _centerLeftPercent = root.Q<Label>("centerLeftPercent");
        _centerRightPercent = root.Q<Label>("centerRightPercent");
        _centerDelta = root.Q<Label>("centerDelta");
        _leftName = root.Q<Label>("leftName");
        _rightName = root.Q<Label>("rightName");
        _leftLead = root.Q<Label>("leftLead");
        _rightLead = root.Q<Label>("rightLead");
        _leftSub = root.Q<Label>("leftSub");
        _rightSub = root.Q<Label>("rightSub");
        _leftBombValue = root.Q<Label>("leftBombValue");
        _leftInkValue = root.Q<Label>("leftInkValue");
        _leftStealValue = root.Q<Label>("leftStealValue");
        _rightBombValue = root.Q<Label>("rightBombValue");
        _rightInkValue = root.Q<Label>("rightInkValue");
        _rightStealValue = root.Q<Label>("rightStealValue");
        _leftValue = root.Q<Label>("leftValue");
        _rightValue = root.Q<Label>("rightValue");
        _leftFill = root.Q<VisualElement>("leftFill");
        _rightFill = root.Q<VisualElement>("rightFill");
        _leftShine = root.Q<VisualElement>("leftShine");
        _rightShine = root.Q<VisualElement>("rightShine");
        _leftBar = root.Q<VisualElement>("leftBar");
        _rightBar = root.Q<VisualElement>("rightBar");

        _bound = _timerText != null;
    }

    private void ApplyFontIfNeeded()
    {
        if (_document == null)
            return;
        var root = _document.rootVisualElement;
        if (root == null)
            return;

        if (_runtimeFont == null)
            _runtimeFont = ResolveFont();
        if (_runtimeFont == null)
            return;

        try
        {
            var def = new FontDefinition();
            object boxed = def;
            var fontProp = boxed.GetType().GetProperty("font");
            if (fontProp != null && fontProp.CanWrite && fontProp.PropertyType == typeof(Font))
                fontProp.SetValue(boxed, _runtimeFont);
            def = (FontDefinition)boxed;
            root.style.unityFontDefinition = new StyleFontDefinition(def);
        }
        catch { }
    }

    private Font ResolveFont()
    {
        Font font = null;
        try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { font = null; }
        if (font != null) return font;
        try { font = Font.CreateDynamicFontFromOSFont(new string[] { "Segoe UI", "Arial", "Helvetica" }, 16); } catch { font = null; }
        if (font != null) return font;
        var all = Resources.FindObjectsOfTypeAll<Font>();
        if (all != null && all.Length > 0) return all[0];
        return null;
    }

    private void Update()
    {
        if (!_bound)
            TryInitializeAndBind();

        if (_gridManager == null)
            _gridManager = FindFirstObjectByType<GridManagerStategy>();
        if (_statsManager == null)
            _statsManager = GameStatsManager.Instance != null ? GameStatsManager.Instance : FindFirstObjectByType<GameStatsManager>();

        if (_remaining > 0f)
        {
            _remaining -= Time.deltaTime;
            if (_remaining < 0f) _remaining = 0f;
        }

        UpdateHUD();
        UpdateShines();
        UpdatePulse();
    }

    private void UpdatePulse()
    {
        float dt = Time.deltaTime;
        if (_p1Pulse > 0f) _p1Pulse -= dt;
        if (_p2Pulse > 0f) _p2Pulse -= dt;

        if (_rightPlate != null)
        {
            float t = Mathf.Clamp01(_p1Pulse / 0.18f);
            float s = 1f + (Mathf.Sin((1f - t) * Mathf.PI) * 0.035f);
            _rightPlate.style.scale = new Scale(new Vector3(s, s, 1f));
        }

        if (_leftPlate != null)
        {
            float t = Mathf.Clamp01(_p2Pulse / 0.18f);
            float s = 1f + (Mathf.Sin((1f - t) * Mathf.PI) * 0.035f);
            _leftPlate.style.scale = new Scale(new Vector3(s, s, 1f));
        }
    }

    private void UpdateHUD()
    {
        int p1 = 0;
        int p2 = 0;
        int total;

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

        _rightPercent = Mathf.Lerp(_rightPercent, targetRight, Time.deltaTime * barLerpSpeed);
        _leftPercent = Mathf.Lerp(_leftPercent, targetLeft, Time.deltaTime * barLerpSpeed);

        if (_rightFill != null) _rightFill.style.width = Length.Percent(_rightPercent * 100f);
        if (_leftFill != null) _leftFill.style.width = Length.Percent(_leftPercent * 100f);

        if (_rightValue != null) _rightValue.text = p1 + "/" + total;
        if (_leftValue != null) _leftValue.text = p2 + "/" + total;

        int p1Pct = Mathf.Clamp(Mathf.RoundToInt(targetRight * 100f), 0, 100);
        int p2Pct = Mathf.Clamp(Mathf.RoundToInt(targetLeft * 100f), 0, 100);
        if (_centerRightPercent != null) _centerRightPercent.text = p1Pct + "%";
        if (_centerLeftPercent != null) _centerLeftPercent.text = p2Pct + "%";

        bool p1Lead = p1 > p2;
        bool p2Lead = p2 > p1;
        if (_rightPlate != null) _rightPlate.EnableInClassList("is-leader", p1Lead);
        if (_leftPlate != null) _leftPlate.EnableInClassList("is-leader", p2Lead);
        if (_rightLead != null) _rightLead.style.display = p1Lead ? DisplayStyle.Flex : DisplayStyle.None;
        if (_leftLead != null) _leftLead.style.display = p2Lead ? DisplayStyle.Flex : DisplayStyle.None;

        if (p1Lead && !_lastP1Lead) _p1Pulse = 0.18f;
        if (p2Lead && !_lastP2Lead) _p2Pulse = 0.18f;
        _lastP1Lead = p1Lead;
        _lastP2Lead = p2Lead;

        if (_centerDelta != null)
        {
            if (p1 == p2)
                _centerDelta.text = "TIE";
            else if (p1Lead)
                _centerDelta.text = "+" + (p1 - p2) + " P1";
            else
                _centerDelta.text = "+" + (p2 - p1) + " P2";
        }

        int p1Bomb = 0;
        int p1Ink = 0;
        int p1Steal = 0;
        int p2Bomb = 0;
        int p2Ink = 0;
        int p2Steal = 0;

        if (_statsManager != null)
        {
            if (_statsManager.BombsPlaced != null && _statsManager.BombsPlaced.Length >= 2)
            {
                p1Bomb = _statsManager.BombsPlaced[0];
                p2Bomb = _statsManager.BombsPlaced[1];
            }
            if (_statsManager.TilesPainted != null && _statsManager.TilesPainted.Length >= 2)
            {
                p1Ink = _statsManager.TilesPainted[0];
                p2Ink = _statsManager.TilesPainted[1];
            }
            if (_statsManager.TilesStolen != null && _statsManager.TilesStolen.Length >= 2)
            {
                p1Steal = _statsManager.TilesStolen[0];
                p2Steal = _statsManager.TilesStolen[1];
            }
        }

        if (_rightSub != null) _rightSub.text = "BOMB " + p1Bomb + "   INK " + p1Ink + "   STEAL " + p1Steal;
        if (_leftSub != null) _leftSub.text = "BOMB " + p2Bomb + "   INK " + p2Ink + "   STEAL " + p2Steal;

        if (_rightBombValue != null) _rightBombValue.text = p1Bomb.ToString();
        if (_rightInkValue != null) _rightInkValue.text = p1Ink.ToString();
        if (_rightStealValue != null) _rightStealValue.text = p1Steal.ToString();
        if (_leftBombValue != null) _leftBombValue.text = p2Bomb.ToString();
        if (_leftInkValue != null) _leftInkValue.text = p2Ink.ToString();
        if (_leftStealValue != null) _leftStealValue.text = p2Steal.ToString();

        if (_timerText != null)
        {
            TimeSpan ts = TimeSpan.FromSeconds(_remaining);
            _timerText.text = string.Format("{0:D2}:{1:D2}", ts.Minutes, ts.Seconds);
        }

        if (_centerPlate != null)
        {
            bool urgent = _remaining <= 30f && _remaining > 10f;
            bool critical = _remaining <= 10f;
            _centerPlate.EnableInClassList("is-urgent", urgent);
            _centerPlate.EnableInClassList("is-critical", critical);
        }
    }

    private void UpdateShines()
    {
        if (_leftShine != null && _leftBar != null)
        {
            float w = _leftBar.resolvedStyle.width;
            if (w > 1f)
            {
                float x = Mathf.Repeat(Time.time * shineSpeed, w + 440f) - 220f;
                _leftShine.style.left = new Length(x, LengthUnit.Pixel);
            }
        }

        if (_rightShine != null && _rightBar != null)
        {
            float w = _rightBar.resolvedStyle.width;
            if (w > 1f)
            {
                float x = Mathf.Repeat(Time.time * shineSpeed, w + 440f) - 220f;
                _rightShine.style.left = new Length(-x, LengthUnit.Pixel);
            }
        }
    }

    private void ApplyPlayerVisuals()
    {
        Color p1 = Color.white;
        Color p2 = Color.white;
        if (Player.PlayerColorDict.TryGetValue(PlayerEnum.Player1, out var c1)) p1 = c1;
        if (Player.PlayerColorDict.TryGetValue(PlayerEnum.Player2, out var c2)) p2 = c2;

        if (_rightName != null)
        {
            _rightName.text = "PLAYER 1";
            _rightName.style.color = new StyleColor(p1);
        }

        if (_leftName != null)
        {
            _leftName.text = "PLAYER 2";
            _leftName.style.color = new StyleColor(p2);
        }

        if (_rightFill != null) _rightFill.style.backgroundColor = new StyleColor(p1);
        if (_leftFill != null) _leftFill.style.backgroundColor = new StyleColor(p2);

        ApplyInkTextures(p1, p2);

        if (_rightAccent != null) _rightAccent.style.backgroundColor = new StyleColor(new Color(p1.r, p1.g, p1.b, 0.95f));
        if (_leftAccent != null) _leftAccent.style.backgroundColor = new StyleColor(new Color(p2.r, p2.g, p2.b, 0.95f));

        if (_centerRightPercent != null) _centerRightPercent.style.color = new StyleColor(p1);
        if (_centerLeftPercent != null) _centerLeftPercent.style.color = new StyleColor(p2);

        if (_centerGlow != null)
        {
            var glow = Color.Lerp(p1, p2, 0.5f);
            glow.a = 0.10f;
            _centerGlow.style.backgroundColor = new StyleColor(glow);
        }
    }

    private void ApplyInkTextures(Color p1, Color p2)
    {
        if (_p1Ink == null)
            _p1Ink = CreateInkTexture(p1, 11);
        if (_p2Ink == null)
            _p2Ink = CreateInkTexture(p2, 22);

        try
        {
            if (_rightFill != null && _p1Ink != null)
                _rightFill.style.backgroundImage = new StyleBackground(_p1Ink);
            if (_leftFill != null && _p2Ink != null)
                _leftFill.style.backgroundImage = new StyleBackground(_p2Ink);
        }
        catch { }
    }

    private Texture2D CreateInkTexture(Color baseColor, int seed)
    {
        int w = 256;
        int h = 32;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Bilinear;

        float br = Mathf.Clamp01(baseColor.r);
        float bg = Mathf.Clamp01(baseColor.g);
        float bb = Mathf.Clamp01(baseColor.b);

        for (int y = 0; y < h; y++)
        {
            float v = y / (float)(h - 1);
            float shade = Mathf.Lerp(0.88f, 1.12f, v);
            for (int x = 0; x < w; x++)
            {
                float u = x / (float)(w - 1);
                float n1 = Mathf.PerlinNoise((x + seed) * 0.045f, (y + seed) * 0.22f);
                float n2 = Mathf.PerlinNoise((x + seed) * 0.14f, (y + seed) * 0.55f);
                float n = Mathf.Lerp(n1, n2, 0.55f);

                float stripes = Mathf.Abs(Mathf.Sin((u * 10.0f + v * 2.4f) * Mathf.PI));
                stripes = Mathf.SmoothStep(0.1f, 0.9f, stripes);
                float ink = Mathf.Lerp(0.78f, 1.22f, n) * Mathf.Lerp(0.96f, 1.04f, stripes);

                float edgeDark = Mathf.Lerp(0.92f, 1.05f, Mathf.SmoothStep(0.05f, 0.95f, v));
                float a = Mathf.Lerp(0.88f, 1f, Mathf.SmoothStep(0.35f, 0.95f, n));

                tex.SetPixel(x, y, new Color(br * shade * ink * edgeDark, bg * shade * ink * edgeDark, bb * shade * ink * edgeDark, a));
            }
        }
        tex.Apply();
        return tex;
    }
}