using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(-2.12f, 1.35f, -3.35f);
    
    private Transform _camTransform;

    void Start()
    {
        _camTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (target == null || _camTransform == null) return;

        transform.position = target.position + offset;
        transform.rotation = _camTransform.rotation;
    }
}