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
    protected float pulseAmplitude = 0.2f;

    [SerializeField]
    protected float pulseSpeed = 8f;
    
    [SerializeField]
    protected int explosionRange = 3;

    protected readonly Vector2Int[] _directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    protected GridManagerStategy _gridManager;
    protected Vector2Int _bombCoordinates;
    protected Vector3 _initialScale;

    public PlayerEnum associatedPlayer = PlayerEnum.None;

    private void Awake()
    {
        GetManagers();

        if (associatedPlayer == PlayerEnum.None)
        {
            Debug.Log("No player selected on bomb");
        }
        
        _initialScale = transform.localScale;
        _bombCoordinates = GridManagerStategy.WorldToGridCoordinates(transform.position);
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
        foreach (var direction in _directions)
        {
            PaintTilesForDirection(_bombCoordinates, direction);
        }
    }
    
    private void PaintTilesForDirection(Vector2Int bombCoordinates, Vector2Int direction)
    {
        for (int rangeCounter = 0; rangeCounter <= explosionRange; ++rangeCounter)
        {
            Tile tile = _gridManager.GetTileAtCoordinates(bombCoordinates);

            if (tile == null || tile.isObstacle)
                return;

            tile.ChangeTileColor(associatedPlayer);
            bombCoordinates += direction;
        }
    }

    protected virtual void OnDestroy()
    {
        ActiveBombs.Remove(_bombCoordinates);
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
    FastBomb = 2,
    SplashBomb = 3
}