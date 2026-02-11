using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;
    
    [SerializeField]
    private Color playerColor = Color.red;

    [SerializeField] 
    private PlayerEnum playerNb = PlayerEnum.None;
    
    private Vector2 _moveInput;
    
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
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnBomb(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            _bombManager.CreateBomb(transform.position, playerNb,  BombEnum.NormalBomb);
        }
    }

    private void TryPlaceBomb()
    {

    }
    
    private void Update()
    {
        TryPlaceBomb();

        Vector2 curMoveInput = _moveInput.normalized;

        Vector2 move = curMoveInput * (speed * Time.deltaTime);
        transform.position += new Vector3(move.y, 0, -move.x);
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