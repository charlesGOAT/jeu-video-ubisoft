using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int TileCoordinates { get; private set; } = new Vector2Int();

    [SerializeField]
    public bool isObstacle = false;

    private GridManager _gridManager;

    public Color TileColor
    {
        get => GetComponentInChildren<Renderer>().material.color;
        set => GetComponentInChildren<Renderer>().material.color = value;
    }

    [SerializeField]
    public int UnityGridSize = 2;

    void Start()
    {
        _gridManager = FindFirstObjectByType<GridManager>();

        TileCoordinates = new Vector2Int(
            Mathf.RoundToInt(transform.position.x / _gridManager.UnityGridSize), 
            Mathf.RoundToInt(transform.position.z / _gridManager.UnityGridSize)
        );

    }

}
