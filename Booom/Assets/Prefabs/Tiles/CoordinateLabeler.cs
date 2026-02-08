using System.Reflection.Emit;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
[ExecuteAlways]
public class CoordinateLabeler : MonoBehaviour
{
    [SerializeField]
    private Color _color = Color.white;

    private TextMeshPro _label;
    private Vector2Int _coordinates = new Vector2Int();

    private void Awake()
    {
        _label = GetComponent<TextMeshPro>();
        _label.enabled = false;
    }

    void Update()
    {
        if (!Application.isPlaying)
        {
            DisplayCoordinates();
            UpdateObjectName();
            _label.enabled = true;
        }
        else 
        {
            _label.enabled = false;
        }

    }

    private void DisplayCoordinates() 
    {
        _coordinates.x = Mathf.RoundToInt(transform.parent.position.x / GridManager.UNITY_GRID_SIZE);
        _coordinates.y = Mathf.RoundToInt(transform.parent.position.z / GridManager.UNITY_GRID_SIZE);
        _label.color = _color;
        _label.text = _coordinates.ToString();
    }
    private void UpdateObjectName() 
    {
        transform.parent.name = _coordinates.ToString();
    }

}
