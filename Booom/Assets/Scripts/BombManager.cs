using System;
using UnityEngine;

public class BombManager : MonoBehaviour
{
    
    [SerializeField]
    private Bomb bombPrefab;

    [SerializeField]
    private float bombCooldown = 3f;
    
    private GridManagerStategy _gridManager;
    
    private float _nextBombAllowedTime = 0f;
    
    private void Awake()
    {
        GetManagers();
        
        if(bombPrefab == null)
        {
            Debug.LogError("Bomb prefab shouldn't be null deactivating component");
            enabled = false;
        }
    }
    
    public void CreateBomb(Vector3 position, PlayerEnum playerEnum, BombEnum bombEnum)
    {
        if (Time.time < _nextBombAllowedTime)
        {
            return;
        }
        
        Vector2Int gridCoordinates = GridManagerStategy.WorldToGridCoordinates(position);
        Tile tile = _gridManager.GetTileAtCoordinates(gridCoordinates);

        if (tile == null || tile.isObstacle || Bomb.IsBombAt(gridCoordinates))
        {
            return;
        }

        Vector3 worldPosition = GridManagerStategy.GridToWorldPosition(gridCoordinates, tile.transform.position.y);
        Instantiate(bombPrefab, worldPosition, Quaternion.identity);
        _nextBombAllowedTime = Time.time + bombCooldown;
    }

    private void GetManagers()
    {
        _gridManager = FindFirstObjectByType<GridManagerStategy>();
        
        if (_gridManager == null)
        {
            throw new Exception("There's no active grid manager");
        }
    }
}

public enum BombEnum
{
    None = 0,
    NormalBomb = 1,
    SquareBomb = 2
}
