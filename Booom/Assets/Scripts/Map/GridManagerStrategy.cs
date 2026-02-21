using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class GridManagerStrategy : MonoBehaviour
{
    protected Dictionary<Vector2Int, Tile> _tiles = new Dictionary<Vector2Int, Tile>();
    protected Dictionary<Vector2Int, Tile> _ownableTiles = new Dictionary<Vector2Int, Tile>();
    protected Dictionary<Vector2Int, Item> _itemTiles = new Dictionary<Vector2Int, Item>();
    
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

    private void Start()
    {
        CreateGrid();
        SetOwnableTiles();
        capturableTilesCount = _ownableTiles.Count;
        PositionCamera();
    }

    protected abstract void CreateGrid();

    public bool IsItemAtPos(Vector2Int pos)
    {
        return _itemTiles.ContainsKey(pos);
    }
    
    public void AddItemOnGrid(Item item)
    {
        _itemTiles[item.posOnMap] = item;
    }
    
    public void RemoveItemFromGrid(Item item)
    {
        _itemTiles.Remove(item.posOnMap);
    }

    private void SetOwnableTiles()
    {
        foreach (var posTile in _tiles)
        {
            if (!posTile.Value.isObstacle)
            {
                _ownableTiles[posTile.Key] = posTile.Value;
            }
        }
    }

    //A besoin d'un peu de peaufinage mais marche pour l'instant
    //Je peut le faire dans un autre task
    //Manque encore du peaufinage lol
    protected void PositionCamera()
    {
        if (mainCamera == null) return;

        float centerX = (MapUpperLimit.x - ((MapUpperLimit.x - MapLowerLimit.x) / 2f)) * GameConstants.UNITY_GRID_SIZE;
        float centerZ = (MapUpperLimit.y - ((MapUpperLimit.y - MapLowerLimit.y) / 2f)) * GameConstants.UNITY_GRID_SIZE;

        mainCamera.transform.position = new Vector3(centerX - (Height * GameConstants.UNITY_GRID_SIZE) / 2f, ((Width + Height) * GameConstants.UNITY_GRID_SIZE) / 2f, centerZ);
        mainCamera.transform.rotation = Quaternion.Euler(60f, 90f, 0f);
    }

    public Vector3 GetRandomPosOnGridWithNoItem()
    {
        var rand = new System.Random();
        var noItemGrid = _ownableTiles.Where(tile => !IsItemAtPos(tile.Key)).Select(tile => tile.Key).ToArray();
        int ind = rand.Next(0, noItemGrid.Length);
        return GridToWorldPosition(noItemGrid[ind]);
    }
    
    public IEnumerable<Vector2Int> GetPlayerTilesWithNoItem(PlayerEnum player)
    {
        if (player == PlayerEnum.None)
            return _ownableTiles.Keys;
        
        var acquiredTiles = GameManager.Instance.ScoreManager.GetAcquiredTilesByPlayer();
        var tilesWithNoItem = acquiredTiles[(int)player - 1].Where(pos => !IsItemAtPos(pos));
        
        return tilesWithNoItem;
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

