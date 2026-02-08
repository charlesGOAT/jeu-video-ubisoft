using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int TileCoordinates { get; private set; } = new Vector2Int();

    [SerializeField]
    public bool isObstacle = false;

    public Color TileColor
    {
        get => GetComponentInChildren<Renderer>().material.color;
        set => GetComponentInChildren<Renderer>().material.color = value;
    }

    [SerializeField]
    public int UnityGridSize = 2;

    void Start()
    {

        TileCoordinates = new Vector2Int(
            Mathf.RoundToInt(transform.position.x / GridManager.UNITY_GRID_SIZE), 
            Mathf.RoundToInt(transform.position.z / GridManager.UNITY_GRID_SIZE)
        );

    }

}
