using System;
using System.Collections.Generic;
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
    private BombEnum _currentBombType = BombEnum.NormalBomb;
    private int _bombTypeCount;

    private GridManagerStategy _gridManager;
    private BombManager _bombManager;

    private void Awake()
    {
        GetManagers();
        _bombTypeCount = Enum.GetValues(typeof(BombEnum)).Length - 1; // -1 to avoid None
        
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            switch (playerInput.playerIndex)
            {
                case 0:
                    playerNb = PlayerEnum.Player1;
                    playerColor = Color.red;
                    break;
                case 1:
                    playerNb = PlayerEnum.Player2;
                    playerColor = Color.green;
                    break;
                default:
                    playerNb = PlayerEnum.None;
                    break;
            }
        }
    }

    public static readonly Dictionary<PlayerEnum, Color> PlayerColorDict = new Dictionary<PlayerEnum, Color>();  // make it the other way around if we want to test color spreading

    private void Start()
    {
        
        if (playerNb == PlayerEnum.None)
        {
            throw new Exception("Player cannot be set to PlayerEnum.None");
        }
        
        if (!PlayerColorDict.TryAdd(playerNb, playerColor))
        {
            throw new Exception("Player already exists");
        }
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnBomb(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            _bombManager.CreateBomb(transform.position, playerNb,  _currentBombType);
        }
    }
    
    public void OnChangeBomb(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            int nextBomb = ((int)_currentBombType % _bombTypeCount) + 1; // +1 to bring back above 0
            _currentBombType = (BombEnum)nextBomb;
        }
    }
    
    private void Update()
    {
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