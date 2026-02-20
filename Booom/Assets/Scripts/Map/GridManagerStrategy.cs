using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class GridManagerStrategy : MonoBehaviour
{
    protected Dictionary<Vector2Int, Tile> _tiles = new Dictionary<Vector2Int, Tile>();
    public int capturableTilesCount;

    public Vector2Int MapUpperLimit { get; protected set; } = Vector2Int.zero;
    public Vector2Int MapLowerLimit { get; protected set; } = Vector2Int.zero;

    public int Width { get; protected set; } = 0;
    public int Height { get; protected set; } = 0;

    [SerializeField]
    protected Camera mainCamera;

    public virtual Tile GetTileAtCoordinates(Vector2Int vector2Int)
    {
        _tiles.TryGetValue(vector2Int, out Tile tile);
        return tile;
    }

    public static Vector2Int WorldToGridCoordinates(Vector3 worldPosition)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPosition.x / GameConstants.UNITY_GRID_SIZE),
            Mathf.RoundToInt(worldPosition.z / GameConstants.UNITY_GRID_SIZE)
        );
    }

    public static Vector3 GridToWorldPosition(Vector2Int gridCoordinates, float y = 0f)
    {
        return new Vector3(
            gridCoordinates.x * GameConstants.UNITY_GRID_SIZE,
            y,
            gridCoordinates.y * GameConstants.UNITY_GRID_SIZE
        );
    }

    private void Awake()
    {
        CreateGrid();
        capturableTilesCount = _tiles.Count;
        PositionCamera();
    }

    protected abstract void CreateGrid();

    //A besoin d'un peu de peaufinage mais marche pour l'instant
    //Je peut le faire dans un autre task
    //Manque encore du peaufinage lol
    //Manque de peaufinage en tbnk
    protected void PositionCamera()
    {
        if (mainCamera == null) return;

        float centerX = (MapUpperLimit.x - ((MapUpperLimit.x - MapLowerLimit.x) / 2f)) * GameConstants.UNITY_GRID_SIZE;
        float centerZ = (MapUpperLimit.y - ((MapUpperLimit.y - MapLowerLimit.y) / 2f)) * GameConstants.UNITY_GRID_SIZE;

        mainCamera.transform.position = new Vector3(centerX - (Height * GameConstants.UNITY_GRID_SIZE) / 2f, ((Width + Height) * GameConstants.UNITY_GRID_SIZE) / 2f, centerZ);
        mainCamera.transform.rotation = Quaternion.Euler(60f, 90f, 0f);
    }

    public Vector3 GetRandomPosOnGrid()
    {
        var rand = new System.Random();
        int ind = rand.Next(0, _tiles.Count);
        return GridToWorldPosition(_tiles.Keys.ToArray()[ind]);
    }
    
    public HashSet<Vector2Int> GetPlayerTiles(PlayerEnum player)
    {
        if (player == PlayerEnum.None)
            return new HashSet<Vector2Int>();
        
        var acquiredTiles = GameManager.Instance.ScoreManager.GetAcquiredTilesByPlayer();

        return acquiredTiles[(int)player - 1];
    }
    
    private HashSet<Vector2Int> GetAllTilesOwned()
    {
        HashSet<Vector2Int> allTilesOwned = new();
        var acquiredTiles = GameManager.Instance.ScoreManager.GetAcquiredTilesByPlayer();
        
        foreach (var list in acquiredTiles)
        {
            allTilesOwned.UnionWith(list);
        }

        return allTilesOwned;
    }

    private IEnumerable<Vector2Int> GetAllTilesNotOwned()
    {
        HashSet<Vector2Int> allTilesOwned = GetAllTilesOwned();
        return _tiles.Keys.Except(allTilesOwned);
    }
}

