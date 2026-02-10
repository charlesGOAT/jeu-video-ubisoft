using System;
using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [SerializeField]
    private float timer = 3.0f;

    [SerializeField]
    private int explosionRange = 3;

    private GridManagerStategy _gridManager;
    
    public Color explosionColor = new Color(0.2f, 1f, 0.6f, 1f);
    public PlayerEnum associatedPlayer = PlayerEnum.None;
    
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
                    
                    tile.ChangeTileColor(explosionColor, associatedPlayer);

                }
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
