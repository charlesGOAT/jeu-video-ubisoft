using System;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    private Dictionary<Vector2Int, Tile> _tiles = new Dictionary<Vector2Int, Tile>();
    
    [SerializeField]
    public int UnityGridSize = 2;

    //Marche juste pour les rectangles
    public Vector2Int MapUpperLimit { get; private set; } = Vector2Int.zero;
    public Vector2Int MapLowerLimit { get; private set; } = Vector2Int.zero;

    public Tile GetTileAtCoordinates(Vector2Int vector2Int)
    {
        _tiles.TryGetValue(vector2Int, out Tile tile);
        return tile;
    }

    void Start()
    {
        foreach (Tile tile in FindObjectsByType<Tile>(FindObjectsSortMode.None))
        {
            _tiles[tile.TileCoordinates] = tile;
            MapLowerLimit = Vector2Int.Min(MapLowerLimit, tile.TileCoordinates);
            MapUpperLimit = Vector2Int.Max(MapUpperLimit, tile.TileCoordinates);
        }
    }
}
