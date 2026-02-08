using System;
using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [SerializeField]
    private float _timer = 3.0f;

    private Color _explosionColor = Color.red;

    [SerializeField]
    private int _explosionRange = 3;

    private GridManager _gridManager;

    void Start()
    {
        _gridManager = FindFirstObjectByType<GridManager>();
    }

    public void Fuse()
    {
        StartCoroutine(CountdownAndExplode());
    }

    /// <summary>
    /// Peindre les cases dans une direction jusqu'à ce qu'une case bloquante soit rencontrée
    /// </summary>
    /// <param name="bombCoordinates"></param>
    /// <param name="direction"></param>
    private void PaintTilesForDirection(Vector2Int bombCoordinates, Vector2Int direction) 
    {
        bool isExplosionBlocked = false;
        int rangeCounter = 0;

        for (Vector2Int nextCoordinates = bombCoordinates; !isExplosionBlocked; nextCoordinates += direction)
        {
               
                Tile tile = _gridManager.GetTileAtCoordinates(nextCoordinates);

                if (tile != null && !tile.isObstacle && rangeCounter <= _explosionRange)
                {
                    tile.TileColor = _explosionColor;
                }
                else
                {
                    isExplosionBlocked = true;
                }

            rangeCounter++;
        }

    }

    /// <summary>
    /// Peindre les cases autour de la bombe en croix
    /// </summary>
    private void Explode()
    {
        Vector2Int bombCoordinates = new Vector2Int(
            Mathf.RoundToInt(transform.position.x / GridManager.UNITY_GRID_SIZE),
            Mathf.RoundToInt(transform.position.z / GridManager.UNITY_GRID_SIZE)
        );
        
        Vector2Int[] directions = new Vector2Int[] {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (Vector2Int direction in directions)
        {
            PaintTilesForDirection(bombCoordinates, direction);
        }

        Destroy(gameObject);

    }

    private IEnumerator CountdownAndExplode()
    {
        yield return new WaitForSeconds(_timer);
        Explode();
    }

    private void Awake()
    {
        Fuse();
    }

}
