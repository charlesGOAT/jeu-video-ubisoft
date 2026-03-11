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

    [Header("Camera Framing")]
    [SerializeField]
    private bool autoFrameCamera = true;

    [SerializeField]
    private Vector2 cameraAngles = new Vector2(55f, 45f);

    [SerializeField]
    private float cameraDistanceMultiplier = 1.15f;

    [SerializeField]
    private float cameraMinDistance = 10f;

    [SerializeField]
    private float cameraMaxDistance = 120f;

    [SerializeField]
    private float cameraVerticalPadding = 4f;

    [Header("Dynamic Camera")]
    [SerializeField]
    private bool enableDynamicCamera = true;

    [SerializeField]
    private float dynamicPositionSmoothTime = 0.1f;

    [SerializeField]
    private float dynamicZoomSmoothTime = 0.14f;

    [SerializeField]
    private float dynamicZoomPadding = 1.2f;

    [SerializeField]
    private float dynamicMinDistance = 14f;

    [SerializeField]
    private float dynamicMaxDistance = 100f;
    
    [SerializeField] 
    public Vector2Int[] playerSpawnPoints;

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
            if (!posTile.Value.IsObstacle)
            {
                _ownableTiles[posTile.Key] = posTile.Value;
            }
        }
    }

    //A besoin d'un peu de peaufinage mais marche pour l'instant
    //Je peut le faire dans un autre task
    //Manque encore du peaufinage lol
    //Manque de peaufinage en tbnk
    protected void PositionCamera()
    {
        if (mainCamera == null) return;

        float centerX = (MapUpperLimit.x - ((MapUpperLimit.x - MapLowerLimit.x) / 2f)) * GameConstants.UNITY_GRID_SIZE;
        float centerZ = (MapUpperLimit.y - ((MapUpperLimit.y - MapLowerLimit.y) / 2f)) * GameConstants.UNITY_GRID_SIZE;

        Vector3 mapCenter = new Vector3(centerX, 0f, centerZ);
        Vector3 mapSize = new Vector3(Width * GameConstants.UNITY_GRID_SIZE, 0f, Height * GameConstants.UNITY_GRID_SIZE);

        Quaternion cameraRotation = Quaternion.Euler(cameraAngles.x, cameraAngles.y, 0f);
        mainCamera.transform.rotation = cameraRotation;

        if (!autoFrameCamera)
        {
            return;
        }

        float longestSide = Mathf.Max(mapSize.x, mapSize.z);
        float diagonal = new Vector2(mapSize.x, mapSize.z).magnitude;
        float framingBaseSize = Mathf.Max(longestSide, diagonal * 0.65f);

        mainCamera.orthographic = false;

        float halfFovRad = mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad;
        float requiredDistance = (framingBaseSize * 0.5f) / Mathf.Tan(halfFovRad);
        float cameraDistance = Mathf.Clamp(
            (requiredDistance + cameraVerticalPadding) * cameraDistanceMultiplier,
            cameraMinDistance,
            cameraMaxDistance
        );

        mainCamera.transform.position = mapCenter - (mainCamera.transform.forward * cameraDistance);
        ConfigureDynamicCamera(mapCenter, mapSize);
    }

    private void ConfigureDynamicCamera(Vector3 mapCenter, Vector3 mapSize)
    {
        if (!enableDynamicCamera || mainCamera == null)
        {
            return;
        }

        DynamicArenaCamera dynamicCamera = mainCamera.GetComponent<DynamicArenaCamera>();
        if (dynamicCamera == null)
        {
            dynamicCamera = mainCamera.gameObject.AddComponent<DynamicArenaCamera>();
        }

        dynamicCamera.Configure(
            mapCenter,
            mapSize,
            dynamicMinDistance,
            dynamicMaxDistance,
            dynamicZoomPadding,
            dynamicPositionSmoothTime,
            dynamicZoomSmoothTime
        );
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
            return new []{WorldToGridCoordinates(GetRandomPosOnGridWithNoItem())};

        var acquiredTiles = GameManager.Instance.ScoreManager.GetAcquiredTilesByPlayer();
        var tilesWithNoItem = acquiredTiles[(int)player - 1].Where(pos => !IsItemAtPos(pos));

        var playerTilesWithNoItem = tilesWithNoItem as Vector2Int[] ?? tilesWithNoItem.ToArray();
        if (playerTilesWithNoItem.Length > 0) return playerTilesWithNoItem;
        
        return new []{WorldToGridCoordinates(GetRandomPosOnGridWithNoItem())};
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
