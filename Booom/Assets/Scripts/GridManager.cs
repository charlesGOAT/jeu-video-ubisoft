using System.Collections;
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
    private bool[,] _occupiedCells;

    private void Start()
    {
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        _grid = new GameObject[width, height];
        _occupiedCells = new bool[width, height];

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

    public bool TryWorldToCell(Vector3 worldPosition, out Vector2Int cell)
    {
        cell = default;
        Vector3 local = worldPosition - transform.position;

        int x = Mathf.FloorToInt((local.x / tileSize) + 0.5f);
        int z = Mathf.FloorToInt((local.z / tileSize) + 0.5f);

        if (x < 0 || x >= width || z < 0 || z >= height)
        {
            return false;
        }

        cell = new Vector2Int(x, z);
        return true;
    }

    public Vector3 GetCellCenter(Vector2Int cell)
    {
        return new Vector3(cell.x * tileSize, 0f, cell.y * tileSize) + transform.position;
    }

    public bool IsInBounds(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height;
    }

    public float TileSize => tileSize;

    public bool IsCellOccupied(Vector2Int cell)
    {
        if (_occupiedCells == null) return false;
        if (cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height) return false;
        return _occupiedCells[cell.x, cell.y];
    }

    public bool TryOccupyCell(Vector2Int cell)
    {
        if (_occupiedCells == null) return false;
        if (cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height) return false;
        if (_occupiedCells[cell.x, cell.y]) return false;

        _occupiedCells[cell.x, cell.y] = true;
        return true;
    }

    public void ReleaseCell(Vector2Int cell)
    {
        if (_occupiedCells == null) return;
        if (cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height) return;

        _occupiedCells[cell.x, cell.y] = false;
    }

    public void PaintCell(Vector2Int cell, Color color, float duration)
    {
        if (_grid == null) return;
        if (!IsInBounds(cell)) return;

        GameObject tile = _grid[cell.x, cell.y];
        if (tile == null) return;

        var renderer = tile.GetComponent<Renderer>();
        if (renderer == null) return;

        // Use instance material to avoid modifying shared material
        Material mat = renderer.material;
        Color original = mat.color;
        mat.color = color;

        StartCoroutine(RevertPaint(renderer, original, duration));
    }

    private IEnumerator RevertPaint(Renderer renderer, Color original, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (renderer != null)
        {
            renderer.material.color = original;
        }
    }
}