using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    private static readonly HashSet<Vector2Int> ActiveBombs = new HashSet<Vector2Int>();

    [SerializeField]
    private float timer = 3.0f;

    [SerializeField]
    private float pulseAmplitude = 0.2f;

    [SerializeField]
    private float pulseSpeed = 8f;

    [SerializeField]
    private int explosionRange = 3;

    private GridManagerStategy _gridManager;
  
    public PlayerEnum associatedPlayer = PlayerEnum.None;

    private Vector2Int _bombCoordinates;

    private Vector3 _initialScale;

    private readonly Vector2Int[] _directions = new Vector2Int[] {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

    void Start()
    {
        _gridManager = FindFirstObjectByType<GridManagerStategy>();
        
        if (_gridManager == null)
        {
            throw new Exception("There's no active grid manager");
        }
    }

    public static bool IsBombAt(Vector2Int gridCoordinates)
    {
        return ActiveBombs.Contains(gridCoordinates);
    }

    public void Fuse()
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

    private void PaintTilesForDirection(Vector2Int bombCoordinates, Vector2Int direction) 
    {
        for (int rangeCounter = 0; rangeCounter <= explosionRange; ++rangeCounter)
        {
            Tile tile = _gridManager.GetTileAtCoordinates(bombCoordinates);

            if (tile == null || tile.isObstacle)
            {
                return;
            }

            if (associatedPlayer != PlayerEnum.None)
            {
                PlayerEnum currentTileOwner = tile.CurrentTileOwner;
                if (currentTileOwner != associatedPlayer)
                {
                    if (currentTileOwner != PlayerEnum.None)
                        _gridManager.tilesPerPlayer[(int)currentTileOwner - 1]--;
                
                    _gridManager.tilesPerPlayer[(int)associatedPlayer - 1]++;
                    
                    tile.ChangeTileColor(associatedPlayer);
                }
            }
            
            bombCoordinates += direction;
        }
    }

    private void Explode()
    {
        Vector2Int bombCoordinates = _bombCoordinates;

        foreach (Vector2Int direction in _directions)
        {
            PaintTilesForDirection(bombCoordinates, direction);
        }

        Destroy(gameObject);
    }

    private void Awake()
    {
        _initialScale = transform.localScale;
        _bombCoordinates = GridManagerStategy.WorldToGridCoordinates(transform.position);
        ActiveBombs.Add(_bombCoordinates);
        Fuse();
    }

    private void OnDestroy()
    {
        ActiveBombs.Remove(_bombCoordinates);
    }

}
