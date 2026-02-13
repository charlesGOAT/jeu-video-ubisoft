using System;
using System.Collections.Generic;
using UnityEngine;

public class BombManager : MonoBehaviour
{
    
    [SerializeField]
    private Bomb[] bombPrefabs;

    [SerializeField]
    private float bombCooldown = 3f;
    
    private GridManagerStategy _gridManager;
    
    // Track each Player's bomb cooldown
    private readonly Dictionary<PlayerEnum, float> _nextBombTime = new Dictionary<PlayerEnum, float>(GameConstants.NB_PLAYERS);
    
    private void Awake()
    {
        GetManagers();
        
        if(bombPrefabs == null)
        {
            Debug.LogError("Bomb prefabs shouldn't be empty");
            enabled = false;
        }

        for (int i = 1; i <= GameConstants.NB_PLAYERS; i++)
        {
            _nextBombTime.Add((PlayerEnum)i, 0f);
        }
    }
    
    public void CreateBomb(Vector3 position, PlayerEnum playerEnum, BombEnum bombEnum)
    {
        if (playerEnum == PlayerEnum.None)
            return;
        if (bombEnum == BombEnum.None)
            return;

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

        var bomb = Instantiate(bombPrefabs[(int)bombEnum - 1], worldPosition, Quaternion.identity);
        bomb.associatedPlayer = playerEnum;

        var stats = GameStatsManager.Instance != null ? GameStatsManager.Instance : FindFirstObjectByType<GameStatsManager>();
        if (stats != null)
            stats.OnBombPlaced(playerEnum);
        
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
