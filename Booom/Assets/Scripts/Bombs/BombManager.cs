using System.Collections.Generic;
using UnityEngine;

public class BombManager : MonoBehaviour
{
    [SerializeField]
    private Bomb[] bombPrefabs;

    [SerializeField]
    private float bombCooldown = 3f;

    // Track each Player's bomb cooldown
    private readonly Dictionary<PlayerEnum, float> _nextBombTime = new Dictionary<PlayerEnum, float>(GameConstants.NB_PLAYERS);

    protected virtual void Awake()
    {
        if (bombPrefabs == null)
        {
            Debug.LogError("Bomb prefabs shouldn't be empty");
            enabled = false;
        }

        for (int i = 1; i <= GameConstants.NB_PLAYERS; i++)
        {
            _nextBombTime.Add((PlayerEnum)i, 0f);
        }
    }

    public virtual void CreateBomb(Vector3 position, PlayerEnum playerEnum, BombEnum bombEnum)
    {
        if (Time.time < _nextBombTime[playerEnum])
        {
            return;
        }

        Vector3 bombHeight = Vector3.up * position.y;
        Vector2Int gridCoordinates = GridManagerStrategy.WorldToGridCoordinates(position);
        Tile tile = GameManager.Instance.GridManager.GetTileAtCoordinates(gridCoordinates);

        if (tile == null || tile.IsObstacle || Bomb.IsBombAt(gridCoordinates))
        {
            return;
        }

        Vector3 worldPosition = GridManagerStrategy.GridToWorldPosition(gridCoordinates, tile.transform.position.y);
        bombPrefabs[(int)bombEnum - 1].associatedPlayer = playerEnum;

        Instantiate(bombPrefabs[(int)bombEnum - 1], worldPosition + bombHeight, Quaternion.identity);

        _nextBombTime[playerEnum] = Time.time + bombCooldown;
    }
}
