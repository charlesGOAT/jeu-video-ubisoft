using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GridManagerStrategy : MonoBehaviour
{
    protected Dictionary<Vector2Int, Tile> _tiles = new Dictionary<Vector2Int, Tile>();
    protected Dictionary<Vector2Int, Tile> _ownableTiles = new Dictionary<Vector2Int, Tile>();
    protected Dictionary<Vector2Int, Item> _itemTiles = new Dictionary<Vector2Int, Item>();
    
    public int CapturableTilesCount;

    public Vector2Int MapUpperLimit { get; protected set; } = Vector2Int.zero;
    public Vector2Int MapLowerLimit { get; protected set; } = Vector2Int.zero;

    public int Width { get; protected set; } = 0;
    public int Height { get; protected set; } = 0;

    [SerializeField]
    protected Camera mainCamera;
    
    [SerializeField] 
    public Vector2Int[] playerSpawnPoints;

    private GameUIManager _uiManager;
    private bool _isCameraInPlace = false;
    private bool _isUIManagerNull = true;
    private float _startTime;

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
        // _uiManager = FindAnyObjectByType<GameUIManager>(FindObjectsInactive.Exclude);
        // if (_uiManager != null)
        // {
        //     _uiManager.gameObject.SetActive(false);
        //     _isUIManagerNull = false;
        // }
        CreateGrid();
        SetOwnableTiles();
        CapturableTilesCount = _ownableTiles.Count - (LobbyManager.JoinedPlayers.Count - 1);
        PositionCamera();
        _startTime = Time.time;

        _isCameraInPlace = !RoundManager.MapsToPlay.Contains(SceneManager.GetActiveScene().buildIndex);
    }

    private void Update()
    {
        // if (Time.time - _startTime >= 0.25f && !_isCameraInPlace && !_isUIManagerNull)
        // {
        //     mainCamera.fieldOfView -= 20 * Time.deltaTime;
        //     if (mainCamera.fieldOfView <= 60)
        //     {
        //         mainCamera.fieldOfView = 60;
        //         _isCameraInPlace = true;
        //         _uiManager.gameObject.SetActive(true);
        //     }
        // }
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

    private void PositionCamera()
    {
        if (SceneManager.GetActiveScene().name.Equals("Menu") ||
            SceneManager.GetActiveScene().name.Equals("EndGame")) return;
        if (mainCamera == null) return;

        float centerX = ((MapUpperLimit.x + MapLowerLimit.x) / 2f) * GameConstants.UNITY_GRID_SIZE;
        float centerZ = ((MapUpperLimit.y + MapLowerLimit.y) / 2f) * GameConstants.UNITY_GRID_SIZE;
        Vector3 mapCenter = new Vector3(centerX, 0f, centerZ); 

        float mapActualWidth = Width * GameConstants.UNITY_GRID_SIZE;
        float mapActualHeight = Height * GameConstants.UNITY_GRID_SIZE;

        float baseHeight = Mathf.Max(mapActualWidth, mapActualHeight);

        Quaternion camRotation = Quaternion.Euler(75f, 90f, 0f);
        mainCamera.transform.rotation = camRotation;
        Vector3 forwardDir = camRotation * Vector3.forward;

        float fovRad = mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad;

        float requiredDistVertical = (mapActualWidth / 2f) / Mathf.Tan(fovRad);
        float requiredDistHorizontal = (mapActualHeight / 2f) / (Mathf.Tan(fovRad) * mainCamera.aspect);

        float perspectivePadding = 1.15f; // Gives a 15% buffer for the tilted perspective
        requiredDistVertical *= perspectivePadding;
        requiredDistHorizontal *= perspectivePadding;

        float minRequiredDistance = Mathf.Max(requiredDistVertical, requiredDistHorizontal);
        float safeMultiplier = (minRequiredDistance * Mathf.Abs(forwardDir.y)) / baseHeight;

        float zoomMultiplier = safeMultiplier - 0.02f;

        float camHeight = baseHeight * zoomMultiplier;
        float distance = camHeight / Mathf.Abs(forwardDir.y);

        mainCamera.transform.position = mapCenter - (forwardDir * distance);
    
        mainCamera.fieldOfView = SceneManager.GetActiveScene().name.Equals("Tuto") ? 67 : 60f;
    }

    public Vector3 GetRandomPosOnGridWithNoItem()
    {
        var rand = new System.Random();
        var noItemGrid = _ownableTiles.Where(tile => !IsItemAtPos(tile.Key) && !Bomb.IsBombAt(tile.Key)).Select(tile => tile.Key).ToArray();
        int ind = rand.Next(0, noItemGrid.Length);
        return GridToWorldPosition(noItemGrid[ind]);
    }
    
    public IEnumerable<Vector2Int> GetPlayerTilesWithNoItem(PlayerEnum player)
    {
        if (player == PlayerEnum.None)
            return new []{WorldToGridCoordinates(GetRandomPosOnGridWithNoItem())};

        var acquiredTiles = GameManager.Instance.ScoreManager.AcquiredTilesByPlayer;
        var tilesWithNoItem = acquiredTiles[player].Where(pos => !IsItemAtPos(pos) && !Bomb.IsBombAt(pos));

        var playerTilesWithNoItem = tilesWithNoItem as Vector2Int[] ?? tilesWithNoItem.ToArray();
        if (playerTilesWithNoItem.Length > 0) return playerTilesWithNoItem;
        
        return new []{WorldToGridCoordinates(GetRandomPosOnGridWithNoItem())};
    }
}
