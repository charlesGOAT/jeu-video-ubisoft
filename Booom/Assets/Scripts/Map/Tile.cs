using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int TileCoordinates { get; private set; } = new Vector2Int();

    [SerializeField]
    public bool isObstacle = false;

    private Renderer TileRenderer;

    public void ChangeTileColor(Color color) 
    {
        TileRenderer.material.color = color;
    }

    void Awake()
    {
        TileRenderer = GetComponentInChildren<Renderer>();
        TileCoordinates = GridManagerStategy.WorldToGridCoordinates(transform.position);

    }

}
