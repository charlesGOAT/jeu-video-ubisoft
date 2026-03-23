using System.Collections;
using UnityEngine;

public class TileAnimation : MonoBehaviour
{
    [SerializeField]
    private float animationDuration = 0.2f;

    [SerializeField]
    private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Renderer _tileRenderer;
    private Coroutine _activeColorLerp;

    public void Initialize(Renderer tileRenderer)
    {
        _tileRenderer = tileRenderer;
    }

    public void AnimateTileColorChange(in Color newColor)
    {
        if (_tileRenderer == null)
        {
            return;
        }

        if (_activeColorLerp != null)
        {
            StopCoroutine(_activeColorLerp);
        }

        _activeColorLerp = StartCoroutine(AnimateColorTransition(_tileRenderer.material.color, newColor));
    }

    public void SetColorImmediate(in Color color)
    {
        if (_activeColorLerp != null)
        {
            StopCoroutine(_activeColorLerp);
            _activeColorLerp = null;
        }

        if (_tileRenderer != null)
        {
            _tileRenderer.material.color = color;
        }
    }

    private IEnumerator AnimateColorTransition(Color previousColor, Color newColor)
    {
        if (animationDuration <= 0f)
        {
            _tileRenderer.material.color = newColor;
            _activeColorLerp = null;
            yield break;
        }

        float elapsed = 0f;
        Material material = _tileRenderer.material;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / animationDuration);
            float lerpFactor = animationCurve.Evaluate(normalizedTime);
            material.color = Color.Lerp(previousColor, newColor, lerpFactor);
            yield return null;
        }

        material.color = newColor;
        _activeColorLerp = null;
    }
}
