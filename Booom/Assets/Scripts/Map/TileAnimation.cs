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
    private static readonly int TileColor = Shader.PropertyToID("_TileColor");

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

        _activeColorLerp = StartCoroutine(AnimateColorTransition(_tileRenderer.material.GetColor(TileColor), newColor));
    }

    public void AnimateExplosionFeedback(in Color targetColor)
    {
        if (_tileRenderer == null)
        {
            return;
        }

        if (_activeColorLerp != null)
        {
            StopCoroutine(_activeColorLerp);
        }

        _activeColorLerp = StartCoroutine(AnimateColorTransition(Color.white, targetColor, true));
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
            _tileRenderer.material.SetColor(TileColor, color);
        }
    }

    private IEnumerator AnimateColorTransition(Color previousColor, Color newColor, bool setInitialColor = false)
    {
        if (animationDuration <= 0f)
        {
            _tileRenderer.material.SetColor(TileColor, newColor);
            _activeColorLerp = null;
            yield break;
        }

        float elapsed = 0f;
        Material material = _tileRenderer.material;

        if (setInitialColor)
        {
            material.SetColor(TileColor, previousColor);;
        }

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / animationDuration);
            float lerpFactor = animationCurve.Evaluate(normalizedTime);
            material.SetColor(TileColor, Color.Lerp(previousColor, newColor, lerpFactor));
            yield return null;
        }

        material.SetColor(TileColor, newColor);
        _activeColorLerp = null;
    }
}
