using System;
using System.Collections.Generic;
using UnityEngine;

public class BombManager : MonoBehaviour
{
    
    [SerializeField]
    private Bomb bombPrefab;

    [SerializeField]
    private float bombCooldown = 3f;
    
    private GridManagerStategy _gridManager;
    
    // Track each Player's bomb cooldown
    private readonly Dictionary<PlayerEnum, float> _nextBombTime = new Dictionary<PlayerEnum, float>();
    
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
        _nextBombTime.TryAdd(playerEnum, 0f);
        
        if (Time.time < _nextBombTime[playerEnum])
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
        
        _nextBombTime[playerEnum] = Time.time + bombCooldown;
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
