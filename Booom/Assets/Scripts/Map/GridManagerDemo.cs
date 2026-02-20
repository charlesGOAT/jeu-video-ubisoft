using UnityEngine;

public class GridManagerDemo : GridManagerStrategy
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

        CreateWalls();
    }

    private void CreateWalls()
    {
        float mapWidth = demoWidth * GameConstants.UNITY_GRID_SIZE;
        float mapHeight = demoHeight * GameConstants.UNITY_GRID_SIZE;
        float wallThickness = 1.0f;
        float wallHeight = 5.0f;
        float yPosition = wallHeight / 2.0f;

        CreateWall(new Vector3(mapWidth / 2 - 1.0f, yPosition, -wallThickness / 2 - 1.0f),
                     new Vector3(mapWidth, wallHeight, wallThickness), "RightWall");

        CreateWall(new Vector3(mapWidth / 2 - 1.0f, yPosition, mapHeight + wallThickness / 2 - 1.0f),
                     new Vector3(mapWidth, wallHeight, wallThickness),  "LeftWall");

        CreateWall(new Vector3(-wallThickness / 2 - 1.0f, yPosition, mapHeight / 2 - 1.0f),
                     new Vector3(wallThickness, wallHeight, mapHeight), "BottomWall");

        CreateWall(new Vector3(mapWidth + wallThickness / 2 - 1.0f, yPosition, mapHeight / 2 - 1.0f),
                     new Vector3(wallThickness, wallHeight, mapHeight), "TopWall");
    }

    private void CreateWall(Vector3 position, Vector3 scale, string objectName)
    {
        GameObject wall = new GameObject(objectName);
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.transform.parent = transform;
        wall.AddComponent<BoxCollider>();
    }
}
