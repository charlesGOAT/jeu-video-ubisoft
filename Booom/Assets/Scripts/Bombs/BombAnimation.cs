using System.Collections;
using System.Linq;
using UnityEngine;

public class BombAnimation : MonoBehaviour
{
    [SerializeField]
    private AnimationCurve animationCurve;

    [SerializeField]
    private AnimationCurve emissionIntensityCurve;

    [SerializeField]
    private GameObject explosionPrefab;
    
    [SerializeField]
    private float changeColorInterval = 0.5f;

    private Bomb _bomb;

    private AnimationCurve _materialAnimationCurve;

    private AnimationCurve _scaleAnimationCurve;

    private int _activeDiscoMaterial = 0;

    private int[] _changingDiscoSquaresSlots;

    private float _animationDuration;

    private float _animationCurrentTime = 0f;

    //a changer pour la couleur de la tuile
    //devra refactor l'explosion selon la couleur aussi
    private Color _changingDiscoSquaresColor = new Color(1, 0.1607843137254902f, 0.4588235294117647f);
    private Color _baseColor;
    private Color[] _originalColors;
    private Renderer _renderer;

    //Pour une raison inconnue (probablement un criss d'epais), le scale dans l'animation curve est x = 1, y = 0.01
    //configfiles?
    private const int YFACTOR = 10;
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _bomb = GetComponent<Bomb>();
        _bomb.OnExplode += Explode;
        EnableEmissionKeyword();
    }

    private void Start()
    {
        CacheDiscoMaterials();
        StartCoroutine(ChangeDiscoColorsCoroutine());
    }

    private void Update()
    {
        UpdateScale();
        UpdateColors();
        _animationCurrentTime += Time.deltaTime;
    }

    private void UpdateScale()
    {
        transform.localScale = Vector3.one * _scaleAnimationCurve.Evaluate(_animationCurrentTime);
    }

    private void InitializeMaterialAnimationCurve()
    {
        _materialAnimationCurve = new(animationCurve.keys.Select(k => new Keyframe(k.time * _animationDuration, k.value * YFACTOR)).ToArray());
    }

    private void InitializeScaleAnimationCurve()
    {
        _scaleAnimationCurve = new(animationCurve.keys.Select(k => new Keyframe(k.time * _animationDuration, 1 + (k.value * YFACTOR))).ToArray());
    }

    private void InitializeEmissionIntensityCurve()
    {
        emissionIntensityCurve = new(emissionIntensityCurve.keys.Select(k => new Keyframe(k.time * _animationDuration, k.value * YFACTOR)).ToArray());
    }

    private void CacheDiscoMaterials()
    {
        Material[] materials = _renderer.materials;
        _changingDiscoSquaresSlots = Enumerable.Range(1, 3)
            .Select(patternIndex => System.Array.FindIndex(materials, x => x.name.Contains($"DiscoMatPattern{patternIndex}")))
            .ToArray();
        _baseColor = materials[_changingDiscoSquaresSlots[0]].color;
        _originalColors = materials.Select(m => m.color).ToArray();
    }

    private void EnableEmissionKeyword()
    {
        foreach (Material material in _renderer.materials)
        {
            material.EnableKeyword("_EMISSION");
        }
    }

    private float GetEmissionIntensity()
    {
        return emissionIntensityCurve.Evaluate(_animationCurrentTime);
    }

    private Color GetEmissionColor(Color materialColor, float lerpFactor, float emissionIntensity)
    {
        return Color.Lerp(Color.black, materialColor, lerpFactor) * emissionIntensity;
    }


    private float GetMaterialLerpFactor()
    {
        return _materialAnimationCurve.Evaluate(_animationCurrentTime);
    }


    private void ChangeDiscoColors()
    {
        if (_changingDiscoSquaresSlots == null || _changingDiscoSquaresSlots.Length == 0)
        {
            return;
        }

        Material[] materials = _renderer.materials;
        int currentSlot = _changingDiscoSquaresSlots[_activeDiscoMaterial];
        Material currentMaterial = materials[currentSlot];
        float lerpFactor = GetMaterialLerpFactor();
        float emissionIntensity = GetEmissionIntensity();

        currentMaterial.color = Color.Lerp(_originalColors[currentSlot], _changingDiscoSquaresColor, lerpFactor);
        currentMaterial.SetColor(EmissionColorId, GetEmissionColor(currentMaterial.color, lerpFactor, emissionIntensity));

        _activeDiscoMaterial = (_activeDiscoMaterial + 1) % _changingDiscoSquaresSlots.Length;
        Material nextMaterial = materials[_changingDiscoSquaresSlots[_activeDiscoMaterial]];
        nextMaterial.color = _changingDiscoSquaresColor;
        nextMaterial.SetColor(EmissionColorId, _changingDiscoSquaresColor * emissionIntensity);
    }

    private void UpdateColors()
    {
        Material[] materials = _renderer.materials;
        float lerpFactor = GetMaterialLerpFactor();
        float emissionIntensity = GetEmissionIntensity();
        int activeSlot = _changingDiscoSquaresSlots[_activeDiscoMaterial];

        for (int i = 0; i < materials.Length; ++i) 
        {
            Material material = materials[i];
            float currentLerpFactor = i == activeSlot ? 1f : lerpFactor;
            material.color = Color.Lerp(_originalColors[i], _changingDiscoSquaresColor, currentLerpFactor);
            material.SetColor(EmissionColorId, GetEmissionColor(material.color, currentLerpFactor, emissionIntensity));
        }
    }

    private IEnumerator ChangeDiscoColorsCoroutine()
    {
        while (true)
        {
            ChangeDiscoColors();
            yield return new WaitForSeconds(changeColorInterval);
        }
    }

    private void Explode()
    {
        var explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Explosion explosionComponent = explosion.GetComponent<Explosion>();
        if (explosionComponent != null)
        {
            explosionComponent.initializeExplosion(_changingDiscoSquaresColor);
        }

        Destroy(gameObject);
    }

    public void SetBombColor(Color newColor)
    {
        _changingDiscoSquaresColor = newColor;
    }

    public void InitializeAnimation(float duration)
    {
        _animationDuration = duration;
        InitializeMaterialAnimationCurve();
        InitializeScaleAnimationCurve();
        InitializeEmissionIntensityCurve();
    }

}
