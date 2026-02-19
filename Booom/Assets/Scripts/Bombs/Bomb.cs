using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    protected static readonly HashSet<Vector2Int> ActiveBombs = new HashSet<Vector2Int>();

    [SerializeField]
    protected float timer = 3.0f;

    [SerializeField]
    private float pulseAmplitude = 0.2f;

    [SerializeField]
    private float pulseSpeed = 8f;
    
    [SerializeField]
    private int explosionRange = 3;

    private readonly Vector2Int[] _directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };
    private Vector3 _initialScale;
    
    protected Vector2Int _bombCoordinates;

    public PlayerEnum associatedPlayer = PlayerEnum.None;

    private void Awake()
    {
        Transform trans = transform;

        _initialScale = trans.localScale;
        _bombCoordinates = GridManagerStategy.WorldToGridCoordinates(trans.position);
        ActiveBombs.Add(_bombCoordinates);
    }

    protected virtual void Start()
    {
        Fuse();
    }

    public static bool IsBombAt(Vector2Int gridCoordinates)
    {
        return ActiveBombs.Contains(gridCoordinates);
    }

    protected void Fuse()
    {
        StartCoroutine(CountdownAndExplode());
    }

    private IEnumerator CountdownAndExplode()
    {
        float elapsed = 0f;
        while (elapsed < timer)
        {
            float pulse = 1f + (Mathf.Abs(Mathf.Sin(elapsed * pulseSpeed)) * pulseAmplitude);
            transform.localScale = _initialScale * pulse;
            elapsed += Time.deltaTime;
            yield return null;
        }
        Explode();
    }

    private void Explode()
    {
        PaintTiles();
        Destroy(gameObject);
    }

    protected virtual void PaintTiles()
    {

        Tile bombTile = GameManager.Instance.GridManager.GetTileAtCoordinates(_bombCoordinates);
        if (bombTile == null) return;
        PlayerEnum currentOwner = bombTile.CurrentTileOwner;

        PlayerEnum newTileOwner = GameManager.Instance.isSpreadingMode ? currentOwner : associatedPlayer;
        PaintTile(_bombCoordinates,Vector2Int.zero, newTileOwner);

        foreach (var direction in _directions)
        {
            PaintTilesForDirection(_bombCoordinates, direction, newTileOwner);
        }
    }
    
    private void PaintTilesForDirection(Vector2Int bombCoordinates, in Vector2Int direction, PlayerEnum newTileOwner)
    {
        for (int rangeCounter = 1; rangeCounter <= explosionRange; ++rangeCounter)
        {
            bombCoordinates += direction;
            
            bool flowControl = PaintTile(bombCoordinates, direction, newTileOwner);
            if (!flowControl)
            {
                return;
            }
        }
    }

    private bool PaintTile(in Vector2Int bombCoordinates, in Vector2Int direction, PlayerEnum newTileOwner)
    {
        Tile tile = GameManager.Instance.GridManager.GetTileAtCoordinates(bombCoordinates);

        if (tile == null || tile.isObstacle)
        {
            return false;
        }

        tile.ChangeTileColor(newTileOwner);
        HitPlayers(bombCoordinates, direction);
        return true;
    }

    private void HitPlayers(in Vector2Int bombCoordinates, in Vector2Int direction)
    {
        foreach (Player player in Player.ActivePlayers)
        {
            Tile playerTile = player.GetPlayerTile();
            if (playerTile != null && playerTile.TileCoordinates == bombCoordinates)
            {
                player.OnHit(direction);
            }
        }
    }

    protected virtual void OnDestroy()
    {
        ActiveBombs.Remove(_bombCoordinates);
    }

}

public enum BombEnum
{
    None = 0,
    NormalBomb = 1,
    FastBomb = 2,
    SplashBomb = 3
}