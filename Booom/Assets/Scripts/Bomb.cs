using System;
using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [SerializeField]
    private float timer = 3.0f;

    private Color _explosionColor = Color.red;

    [SerializeField]
    private int explosionRange = 3;

    private GridManagerStategy _gridManager;

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

    public void Fuse()
    {
        StartCoroutine(CountdownAndExplode());
    }

    private IEnumerator CountdownAndExplode()
    {
        yield return new WaitForSeconds(timer);
        Explode();
    }

    /// <summary>
    /// Peindre les cases dans une direction jusqu'à ce qu'une case bloquante soit rencontrée
    /// </summary>
    /// <param name="bombCoordinates"></param>
    /// <param name="direction"></param>
    private void PaintTilesForDirection(Vector2Int bombCoordinates, Vector2Int direction) 
    {

        for (int rangeCounter = 0; rangeCounter <= explosionRange; ++rangeCounter)
        {
            
            Tile tile = _gridManager.GetTileAtCoordinates(bombCoordinates);

            if (tile != null && !tile.isObstacle)
            {
                tile.ChangeTileColor(_explosionColor);
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
        Vector2Int bombCoordinates = GridManagerStategy.WorldToGridCoordinates(transform.position);

        foreach (Vector2Int direction in _directions)
        {
            PaintTilesForDirection(bombCoordinates, direction);
        }

        Destroy(gameObject);

    }

    private void Awake()
    {
        Fuse();
    }

}
