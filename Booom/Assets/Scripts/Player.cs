using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;

    [SerializeField]
    private Bomb bombPrefab;

    [SerializeField]
    private float bombCooldown = 3f;
    
    private Vector2 _moveInput;

    private float _nextBombAllowedTime = 0f;
    private GridManagerStategy _gridManager;

    private void Start()
    {
        _gridManager = FindFirstObjectByType<GridManagerStategy>();
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
        if (Time.time < _nextBombAllowedTime || bombPrefab == null)
        {
            return;
        }

        if (_gridManager == null)
        {
            _gridManager = FindFirstObjectByType<GridManagerStategy>();
            if (_gridManager == null)
            {
                return;
            }
        }

        Vector2Int gridCoordinates = GridManagerStategy.WorldToGridCoordinates(transform.position);
        Tile tile = _gridManager.GetTileAtCoordinates(gridCoordinates);

        if (tile == null || tile.isObstacle)
        {
            return;
        }

        if (Bomb.IsBombAt(gridCoordinates))
        {
            return;
        }

        Vector3 worldPosition = GridManagerStategy.GridToWorldPosition(gridCoordinates, tile.transform.position.y);
        Instantiate(bombPrefab, worldPosition, Quaternion.identity);
        _nextBombAllowedTime = Time.time + bombCooldown;
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryPlaceBomb();
        }

        Vector2 curMoveInput = _moveInput.normalized;

        Vector2 move = curMoveInput * (speed * Time.deltaTime);
        transform.position += new Vector3(move.y, 0, -move.x);
    }
}