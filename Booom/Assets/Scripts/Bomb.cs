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
    private Color explosionColor = new Color(0.2f, 1f, 0.6f, 1f);

    [SerializeField]
    private float pulseAmplitude = 0.2f;

    [SerializeField]
    private float pulseSpeed = 8f;

    [SerializeField]
    private int explosionRange = 3;

    private GridManagerStategy _gridManager;

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

    /// <summary>
    /// Peindre les cases dans une direction jusqu'� ce qu'une case bloquante soit rencontr�e
    /// </summary>
    /// <param name="bombCoordinates"></param>
    /// <param name="direction"></param>
    private void PaintTilesForDirection(Vector2Int bombCoordinates, Vector2Int direction) 
    {

        if (_gridManager == null)
        {
            return;
        }

        for (int rangeCounter = 0; rangeCounter <= explosionRange; ++rangeCounter)
        {
            
            Tile tile = _gridManager.GetTileAtCoordinates(bombCoordinates);

            if (tile != null && !tile.isObstacle)
            {
                tile.ChangeTileColor(explosionColor);
            }
            else
            {
                return;
            }

            bombCoordinates += direction;
        }

    }

    /// <summary>
    /// Peindre les cases autour de la bombe en croix
    /// </summary>
    private void Explode()
    {
        if (_gridManager == null)
        {
            _gridManager = FindFirstObjectByType<GridManagerStategy>();
            if (_gridManager == null)
            {
                Destroy(gameObject);
                return;
            }
        }

        Vector2Int bombCoordinates = _bombCoordinates;

        foreach (Vector2Int direction in _directions)
        {
            PaintTilesForDirection(bombCoordinates, direction);
        }

        Destroy(gameObject);

    }

    private void Awake()
    {
        if (_gridManager == null)
        {
            _gridManager = FindFirstObjectByType<GridManagerStategy>();
        }

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
