using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField]
    private int width = 15;
    [SerializeField]
    private int height = 15;
    [SerializeField]
    private float tileSize = 1f;
    [SerializeField]
    private GameObject tilePrefab;
    [SerializeField]
    private GameObject mainCamera;

    private GameObject[,] _grid;

    private void Start()
    {
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        _grid = new GameObject[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 position = new Vector3(
                    x * tileSize,
                    0,
                    z * tileSize
                );

                GameObject tile = Instantiate(
                    tilePrefab,
                    position,
                    Quaternion.identity,
                    transform
                );

                _grid[x, z] = tile;
            }
        }

        PositionCamera();
    }
    
    private void PositionCamera()
    {
        if (mainCamera == null) return;

        float centerX = (width - 1) * tileSize / 2f;
        float centerZ = (height - 1) * tileSize / 2f;

        mainCamera.transform.position = new Vector3(centerX - height / 2f, (width + height) / 2f, centerZ);
        mainCamera.transform.rotation = Quaternion.Euler(60f, 90f, 0f);
    }
}