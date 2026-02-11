using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;

    [SerializeField]
    private Bomb bombPrefab;

    [SerializeField]
    private float bombCooldown = 3f;
    
    [SerializeField]
    private Color playerColor = Color.red;

    [SerializeField] 
    private PlayerEnum playerNb = PlayerEnum.None;
    
    private Vector2 _moveInput;

    private float _nextBombAllowedTime = 0f;
    private GridManagerStategy _gridManager;
    private BombManager _bombManager;
    
    private void Awake()
    {
        GetManagers();
        
        // if(bombPrefab == null)
        // {
        //     Debug.LogError("Bomb prefab shouldn't be null deactivating component");
        //     enabled = false;
        // }
        //
        // bombPrefab.explosionColor = playerColor;
        // bombPrefab.associatedPlayer = playerNb;

    public static readonly Dictionary<PlayerEnum, Color> PlayerColorDict = new Dictionary<PlayerEnum, Color>();  // make it the other way around if we want to test color spreading

    private void Start()
    {
        _gridManager = FindFirstObjectByType<GridManagerStategy>();

        if (_gridManager == null)
        {
            throw new Exception("There's no active grid manager");
        }
        
        if(bombPrefab == null)
        {
            Debug.LogError("Bomb prefab shouldn't be null deactivating component");
            enabled = false;
        }

        if (playerNb == PlayerEnum.None)
        {
            throw new Exception("Player cannot be set to PlayerEnum.None");
        }

        if (PlayerColorDict.ContainsKey(playerNb))
        {
            throw new Exception("Two players can't have the same player number.");
        }

        PlayerColorDict[playerNb] = playerColor;
        bombPrefab.associatedPlayer = playerNb;
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnBomb(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            TryPlaceBomb();
        }
    }

    private void TryPlaceBomb()
    {
        if (Time.time < _nextBombAllowedTime)
        {
            return;
        }
        
        Vector2Int gridCoordinates = GridManagerStategy.WorldToGridCoordinates(transform.position);
        Tile tile = _gridManager.GetTileAtCoordinates(gridCoordinates);

        if (tile == null || tile.isObstacle || Bomb.IsBombAt(gridCoordinates))
        {
            return;
        }

        Vector3 worldPosition = GridManagerStategy.GridToWorldPosition(gridCoordinates, tile.transform.position.y);
        Instantiate(bombPrefab, worldPosition, Quaternion.identity);
        _nextBombAllowedTime = Time.time + bombCooldown;
    }
    
    private void Update()
    {
        TryPlaceBomb();

        Vector2 curMoveInput = _moveInput.normalized;

        Vector2 move = curMoveInput * (speed * Time.deltaTime);
        transform.position += new Vector3(move.y, 0, -move.x);
    }

    private bool CheckIfOnOwnColor()
    {
        Vector2Int gridCoordinates = GridManagerStategy.WorldToGridCoordinates(transform.position);
        Tile tile = _gridManager.GetTileAtCoordinates(gridCoordinates);
        if (tile == null)
        {
            return false;
        }

        return tile.CurrentTileOwner == playerNb;
    }

    private void GetManagers()
    {
        _gridManager = FindFirstObjectByType<GridManagerStategy>();
        _bombManager = FindFirstObjectByType<BombManager>();

        if (_gridManager == null)
        {
            throw new Exception("There's no active grid manager");
        }
        if (_bombManager == null)
        {
            throw new Exception("There's no active grid manager");
        }
    }
}

public enum PlayerEnum
{
    None = 0,
    Player1 = 1,
    Player2 = 2 // add more
}