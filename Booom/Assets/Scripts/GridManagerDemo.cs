using UnityEngine;

public class GridManagerDemo : GridManagerStategy
{
    [SerializeField]
    private int _demoWidth = 10;

    [SerializeField]
    private int _demoHeight = 10;

    [SerializeField]
    private GameObject _tilePrefab;

    protected override void CreateGrid()
    {
        for (int x = 0; x < _demoWidth; x++)
        {
            for (int y = 0; y < _demoHeight; y++)
            {
                Vector3 tilePosition = new Vector3(x * UNITY_GRID_SIZE, 0, y * UNITY_GRID_SIZE);
                GameObject tileObject = Instantiate(_tilePrefab, tilePosition, Quaternion.identity, transform);
                Tile tile = tileObject.GetComponent<Tile>();
                _tiles[new Vector2Int(x, y)] = tile;
            }
        }
        
        MapUpperLimit = new Vector2Int(_demoWidth - 1, _demoHeight - 1);
        MapLowerLimit = Vector2Int.zero;

        Width = _demoWidth;
        Height = _demoHeight;
    }



}
