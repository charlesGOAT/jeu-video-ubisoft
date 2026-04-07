using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class RaveText : MonoBehaviour
{
    private TMP_Text _text;

    [Header("State")]
    [SerializeField] public bool isSelected = true;

    [Header("Rave Settings")]
    [SerializeField] private float shakeAmount = 1f;
    [SerializeField] private float colorChangeSpeed = 3f;
    [SerializeField] private float pulseSpeed = 10f;
    [SerializeField] private float pulseScale = 0f;
    [SerializeField] private float basicScale = 2.5f;

    private Color[] playerColors = new Color[]
    {
        new Color(255f / 255f, 41f / 255f, 117f / 255f),
        new Color(0f, 245f / 255f, 212f / 255f),
        new Color(107f / 255f, 44f / 255f, 255f / 255f),
        new Color(255f / 255f, 255f / 255f, 33f / 255f)
    };

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (isSelected)
        {
            AnimateVertices();
            AnimateColors();
            AnimateTransform();
        }
        else
        {
            ResetToDefault();
        }
    }

    void ResetToDefault()
    {
        transform.localScale = Vector3.one * basicScale;

        _text.ForceMeshUpdate();
        var textInfo = _text.textInfo;

        for (int m = 0; m < textInfo.meshInfo.Length; m++)
        {
            var meshInfo = textInfo.meshInfo[m];
            var colors = meshInfo.colors32;

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = new Color(226f / 255f, 226f / 255f, 226f / 255f);
            }

            meshInfo.mesh.colors32 = colors;
            _text.UpdateGeometry(meshInfo.mesh, m);
        }
    }

    void AnimateTransform()
    {
        float scale = basicScale + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseScale;
        transform.localScale = Vector3.one * scale;
    }

    void AnimateColors()
    {
        _text.ForceMeshUpdate();
        var textInfo = _text.textInfo;

        float t = Time.unscaledTime * colorChangeSpeed;
        int colorIndex = Mathf.FloorToInt(t) % playerColors.Length;
        Color32 c32 = playerColors[colorIndex];

        for (int m = 0; m < textInfo.meshInfo.Length; m++)
        {
            var meshInfo = textInfo.meshInfo[m];
            var colors = meshInfo.colors32;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                var charInfo = textInfo.characterInfo[i];
            
                if (!charInfo.isVisible || charInfo.materialReferenceIndex != m)
                    continue;

                int index = charInfo.vertexIndex;

                colors[index + 0] = c32;
                colors[index + 1] = c32;
                colors[index + 2] = c32;
                colors[index + 3] = c32;
            }

            meshInfo.mesh.colors32 = colors;
            _text.UpdateGeometry(meshInfo.mesh, m);
        }
    }

    void AnimateVertices()
    {
        _text.ForceMeshUpdate();
        var textInfo = _text.textInfo;

        for (int m = 0; m < textInfo.meshInfo.Length; m++)
        {
            var meshInfo = textInfo.meshInfo[m];
            var vertices = meshInfo.vertices;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                var charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible || charInfo.materialReferenceIndex != m)
                    continue;

                int index = charInfo.vertexIndex;

                Vector3 jitter = new Vector3(
                    Random.Range(-shakeAmount, shakeAmount),
                    Random.Range(-shakeAmount, shakeAmount),
                    0);

                vertices[index + 0] += jitter;
                vertices[index + 1] += jitter;
                vertices[index + 2] += jitter;
                vertices[index + 3] += jitter;
            }

            meshInfo.mesh.vertices = vertices;
            _text.UpdateGeometry(meshInfo.mesh, m);
        }
    }
}