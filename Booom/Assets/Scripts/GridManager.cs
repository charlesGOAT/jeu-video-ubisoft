using System;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    private Dictionary<Vector2Int, Tile> _tiles = new Dictionary<Vector2Int, Tile>();
    
    public const int UNITY_GRID_SIZE = 2;

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

//    [SerializeField]
//    private int width = 15;
//    [SerializeField]
//    private int height = 15;
//    [SerializeField]
//    private float tileSize = 1f;
//    [SerializeField]
//    private GameObject tilePrefab;
//    [SerializeField]
//    private GameObject mainCamera;

//    private GameObject[,] _grid;

//    private void Start()
//    {
//        GenerateGrid();
//    }

//    private void GenerateGrid()
//    {
//        _grid = new GameObject[width, height];

//        for (int x = 0; x < width; x++)
//        {
//            for (int z = 0; z < height; z++)
//            {
//                Vector3 position = new Vector3(
//                    x * tileSize,
//                    0,
//                    z * tileSize
//                );

//                GameObject tile = Instantiate(
//                    tilePrefab,
//                    position,
//                    Quaternion.identity,
//                    transform
//                );

//                _grid[x, z] = tile;
//            }
//        }

//        PositionCamera();
//    }

//    private void PositionCamera()
//    {
//        if (mainCamera == null) return;

//        float centerX = (width - 1) * tileSize / 2f;
//        float centerZ = (height - 1) * tileSize / 2f;

//        mainCamera.transform.position = new Vector3(centerX - height / 2f, (width + height) / 2f, centerZ);
//        mainCamera.transform.rotation = Quaternion.Euler(60f, 90f, 0f);
//    }
//}
}
