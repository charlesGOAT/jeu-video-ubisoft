using System;
using System.Collections.Generic;
using UnityEngine;

public class BombManager : MonoBehaviour
{
    [SerializeField]
    private Bomb[] bombPrefabs;

    [SerializeField]
    private float bombCooldown = 3f;

    // Track each Player's bomb cooldown
    private readonly Dictionary<PlayerEnum, float> _nextBombTime = new (GameConstants.NB_PLAYERS);

    private readonly Dictionary<PlayerEnum, List<Bomb>> _chainedBombsPerPlayer = new (GameConstants.NB_PLAYERS);

    private void Awake()
    {
        if (bombPrefabs == null)
        {
            Debug.LogError("Bomb prefabs shouldn't be empty");
            enabled = false;
        }

        for (int i = 1; i <= GameConstants.NB_PLAYERS; i++)
        {
            _nextBombTime.Add((PlayerEnum)i, 0f);
            _chainedBombsPerPlayer.Add((PlayerEnum)i, new());
        }
    }

    public bool CreateBomb(Vector3 position, PlayerEnum playerEnum, BombEnum bombEnum, bool isTransparentBomb = false, bool isChained = false)
    {
        if (Time.time < _nextBombTime[playerEnum] && !isChained)
        {
            return false;
        }

        Vector2Int gridCoordinates = GridManagerStategy.WorldToGridCoordinates(position);
        Tile tile = GameManager.Instance.GridManager.GetTileAtCoordinates(gridCoordinates);

        if (tile == null || tile.isObstacle || Bomb.IsBombAt(gridCoordinates))
        {
            return false;
        }

        Vector3 worldPosition = GridManagerStategy.GridToWorldPosition(gridCoordinates, tile.transform.position.y);
        bombPrefabs[(int)bombEnum - 1].associatedPlayer = playerEnum;

        bombPrefabs[(int)bombEnum - 1].isChainedBomb = isChained;
        Bomb instantiatedBomb = Instantiate(bombPrefabs[(int)bombEnum - 1], worldPosition, Quaternion.identity);
        instantiatedBomb.isTransparentBomb = isTransparentBomb;

        if (isChained)
            _chainedBombsPerPlayer[playerEnum].Add(instantiatedBomb);

        _nextBombTime[playerEnum] = Time.time + bombCooldown;

        return true;
    }

    public void ExplodeChainedBombs(PlayerEnum player)
    {
        foreach (Bomb bomb in _chainedBombsPerPlayer[player])
        {
            bomb.Explode();
        }
        
        _chainedBombsPerPlayer[player].Clear();
    }

    public bool HasChainedBombs(PlayerEnum player)
    {
        return _chainedBombsPerPlayer[player].Count != 0;
    }
}
