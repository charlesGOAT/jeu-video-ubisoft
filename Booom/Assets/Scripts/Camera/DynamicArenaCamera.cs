using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class DynamicArenaCamera : MonoBehaviour
{
    private Camera _camera;

    private Vector3 _mapCenter;
    private Vector3 _mapSize;

    private float _minDistance;
    private float _maxDistance;
    private float _zoomPadding;

    private float _positionSmoothTime;
    private float _zoomSmoothTime;
    private float _viewportPadding;
    private float _worldPadding;
    private float _heightPadding;

    private Vector3 _positionVelocity;
    private float _zoomVelocity;

    private float _currentDistance;
    private float _targetDistance;
    private bool _isConfigured;
    private float _introTimer;
    private int _lastScreenWidth;
    private int _lastScreenHeight;
    private float _lastFieldOfView;

    private readonly Vector3[] _framingPoints = new Vector3[8];

    [Header("Map Reveal")]
    [SerializeField]
    private bool enableMapReveal = true;

    [SerializeField]
    private float revealDuration = 1.25f;

    [SerializeField]
    private float revealStartDistanceMultiplier = 1.12f;

    [Header("Professional Framing")]
    [SerializeField, Range(0f, 0.25f)]
    private float viewportPadding = 0.035f;

    [SerializeField]
    private float worldPadding = 0.4f;

    [SerializeField]
    private float obstacleHeightPadding = 1.2f;

    [SerializeField, Range(0.85f, 1.15f)]
    private float framingDistanceBias = 0.94f;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    public void Configure(
        Vector3 mapCenter,
        Vector3 mapSize,
        float minDistance,
        float maxDistance,
        float zoomPadding,
        float positionSmoothTime,
        float zoomSmoothTime)
    {
        _mapCenter = mapCenter;
        _mapSize = mapSize;

        _minDistance = Mathf.Max(0.1f, minDistance);
        _maxDistance = Mathf.Max(_minDistance, maxDistance);

        _zoomPadding = Mathf.Max(0f, zoomPadding);

        _positionSmoothTime = Mathf.Max(0.01f, positionSmoothTime);
        _zoomSmoothTime = Mathf.Max(0.01f, zoomSmoothTime);

        _viewportPadding = Mathf.Clamp01(viewportPadding);
        _worldPadding = Mathf.Max(0f, worldPadding + (_zoomPadding * 0.25f));
        _heightPadding = Mathf.Max(0f, obstacleHeightPadding);

        BuildFramingPoints();
        _targetDistance = Mathf.Clamp(CalculateTargetDistance() * framingDistanceBias, _minDistance, _maxDistance);

        float initialDistance = _targetDistance;
        if (enableMapReveal)
        {
            initialDistance *= Mathf.Max(1f, revealStartDistanceMultiplier);
        }

        _currentDistance = Mathf.Clamp(initialDistance, _minDistance, _maxDistance);
        _introTimer = 0f;
        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;
        _lastFieldOfView = _camera.fieldOfView;
        _isConfigured = true;
    }

    private void LateUpdate()
    {
        if (!_isConfigured || _camera == null)
        {
            return;
        }

        if (_lastScreenWidth != Screen.width || _lastScreenHeight != Screen.height || Mathf.Abs(_lastFieldOfView - _camera.fieldOfView) > 0.001f)
        {
            _targetDistance = Mathf.Clamp(CalculateTargetDistance() * framingDistanceBias, _minDistance, _maxDistance);
            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;
            _lastFieldOfView = _camera.fieldOfView;
        }

        UpdatePerspectiveCamera(_mapCenter);
    }

    private void UpdatePerspectiveCamera(Vector3 desiredFocus)
    {
        _camera.orthographic = false;

        float desiredDistance = _targetDistance;

        if (enableMapReveal && _introTimer < revealDuration)
        {
            _introTimer += Time.deltaTime;
            float revealT = Mathf.Clamp01(_introTimer / Mathf.Max(0.01f, revealDuration));
            float easedRevealT = 1f - Mathf.Pow(1f - revealT, 3f);
            float revealDistance = Mathf.Clamp(desiredDistance * Mathf.Max(1f, revealStartDistanceMultiplier), _minDistance, _maxDistance);
            desiredDistance = Mathf.Lerp(revealDistance, desiredDistance, easedRevealT);
        }

        _currentDistance = Mathf.SmoothDamp(_currentDistance, desiredDistance, ref _zoomVelocity, _zoomSmoothTime);

        Vector3 desiredPosition = desiredFocus - (transform.forward * _currentDistance);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _positionVelocity, _positionSmoothTime);
    }

    private void BuildFramingPoints()
    {
        float halfX = (_mapSize.x * 0.5f) + _worldPadding;
        float halfZ = (_mapSize.z * 0.5f) + _worldPadding;

        Vector3 p1 = new(_mapCenter.x - halfX, _mapCenter.y, _mapCenter.z - halfZ);
        Vector3 p2 = new(_mapCenter.x + halfX, _mapCenter.y, _mapCenter.z - halfZ);
        Vector3 p3 = new(_mapCenter.x - halfX, _mapCenter.y, _mapCenter.z + halfZ);
        Vector3 p4 = new(_mapCenter.x + halfX, _mapCenter.y, _mapCenter.z + halfZ);

        _framingPoints[0] = p1;
        _framingPoints[1] = p2;
        _framingPoints[2] = p3;
        _framingPoints[3] = p4;

        Vector3 yOffset = Vector3.up * _heightPadding;
        _framingPoints[4] = p1 + yOffset;
        _framingPoints[5] = p2 + yOffset;
        _framingPoints[6] = p3 + yOffset;
        _framingPoints[7] = p4 + yOffset;
    }

    private float CalculateTargetDistance()
    {
        float low = _minDistance;
        float high = _maxDistance;

        if (!DoesDistanceFit(high))
        {
            return _maxDistance;
        }

        for (int i = 0; i < 20; i++)
        {
            float mid = (low + high) * 0.5f;
            if (DoesDistanceFit(mid))
            {
                high = mid;
            }
            else
            {
                low = mid;
            }
        }

        return high;
    }

    private bool DoesDistanceFit(float distance)
    {
        Vector3 originalPosition = transform.position;
        Quaternion originalRotation = transform.rotation;

        transform.position = _mapCenter - (transform.forward * distance);

        float min = _viewportPadding;
        float max = 1f - _viewportPadding;

        for (int i = 0; i < _framingPoints.Length; i++)
        {
            Vector3 viewportPoint = _camera.WorldToViewportPoint(_framingPoints[i]);
            if (viewportPoint.z <= 0f || viewportPoint.x < min || viewportPoint.x > max || viewportPoint.y < min || viewportPoint.y > max)
            {
                transform.position = originalPosition;
                transform.rotation = originalRotation;
                return false;
            }
        }

        transform.position = originalPosition;
        transform.rotation = originalRotation;
        return true;
    }

}
