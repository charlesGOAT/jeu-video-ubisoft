using System.Linq;
using UnityEngine;

public class GridManagerDemo : GridManagerStategy
{
    [SerializeField]
    private int demoWidth = 10;

    [SerializeField]
    private int demoHeight = 10;

    [SerializeField]
    private GameObject tilePrefab;

    protected override void CreateGrid()
    {
        for (int x = 0; x < demoWidth; x++)
        {
            for (int y = 0; y < demoHeight; y++)
            {
                Vector3 tilePosition = new Vector3(x * GameConstants.UNITY_GRID_SIZE, 0, y * GameConstants.UNITY_GRID_SIZE);
                GameObject tileObject = Instantiate(tilePrefab, tilePosition, Quaternion.identity, transform);
                Tile tile = tileObject.GetComponent<Tile>();
                _tiles[new Vector2Int(x, y)] = tile;
            }
        }
        
        MapUpperLimit = new Vector2Int(demoWidth - 1, demoHeight - 1);
        MapLowerLimit = Vector2Int.zero;

        Width = demoWidth;
        Height = demoHeight;
    }
}
