using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class GridManagerStategy : MonoBehaviour
{
    protected const int UNITY_GRID_SIZE = 2;

    protected Dictionary<Vector2Int, Tile> _tiles = new Dictionary<Vector2Int, Tile>();

    public Vector2Int MapUpperLimit { get; protected set; } = Vector2Int.zero;
    public Vector2Int MapLowerLimit { get; protected set; } = Vector2Int.zero;

    public int Width { get; protected set; } = 0;
    public int Height { get; protected set; } = 0;
    
    public readonly int[] tilesPerPlayer = new int[] { 0, 0 }; // only 2 for the moment, add game constants and generate dynamically?

    [SerializeField]
    protected Camera mainCamera;

    public Tile GetTileAtCoordinates(Vector2Int vector2Int)
    {
        _tiles.TryGetValue(vector2Int, out Tile tile);
        return tile;
    }

    public static Vector2Int WorldToGridCoordinates(Vector3 worldPosition)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPosition.x / UNITY_GRID_SIZE),
            Mathf.RoundToInt(worldPosition.z / UNITY_GRID_SIZE)
        );
    }

    public static Vector3 GridToWorldPosition(Vector2Int gridCoordinates, float y = 0f)
    {
        return new Vector3(
            gridCoordinates.x * UNITY_GRID_SIZE,
            y,
            gridCoordinates.y * UNITY_GRID_SIZE
        );
    }

    private void Start()
    {
        CreateGrid();
        PositionCamera();
    }

    protected abstract void CreateGrid();

    //A besoin d'un peu de peaufinage mais marche pour l'instant
    //Je peut le faire dans un autre task
    protected void PositionCamera()
    {
        if (mainCamera == null) return;

        float centerX = (MapUpperLimit.x - ((MapUpperLimit.x - MapLowerLimit.x) / 2f)) * UNITY_GRID_SIZE;
        float centerZ = (MapUpperLimit.y - ((MapUpperLimit.y - MapLowerLimit.y) / 2f)) * UNITY_GRID_SIZE;

        mainCamera.transform.position = new Vector3(centerX, ((Width + Height) * UNITY_GRID_SIZE) / 2f, (centerZ - (Height * UNITY_GRID_SIZE)) / 2f);
        mainCamera.transform.rotation = Quaternion.Euler(65f, 0f, 0f);
    }
}

