using UnityEngine;

public class ExplosionFX : MonoBehaviour
{
    public float Duration = 0.25f;
    public float PulseScale = 1.1f;

    private Vector3 _baseScale;
    private float _startTime;

    private void Awake()
    {
        _baseScale = transform.localScale;
        _startTime = Time.time;
    }

    private void Update()
    {
        float t = Mathf.InverseLerp(_startTime, _startTime + Duration, Time.time);
        // Simple pulse: scale up then down
        float pulse = Mathf.Sin(t * Mathf.PI);
        float scaleFactor = Mathf.Lerp(1f, PulseScale, pulse);
        transform.localScale = _baseScale * scaleFactor;
    }
}
