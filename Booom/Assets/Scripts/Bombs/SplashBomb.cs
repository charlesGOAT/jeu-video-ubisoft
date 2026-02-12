using UnityEngine;

public class SplashBomb : Bomb
{
    [SerializeField]
    private int explosionRange = 1;
    
    private readonly Vector2Int[] _offsets =
    {
        new Vector2Int(-1, -1),
        new Vector2Int(0, -1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 0),
        new Vector2Int(1, 0),
        new Vector2Int(-1, 1),
        new Vector2Int(0, 1),
        new Vector2Int(1, 1),
    };

    protected override void Explode()
    {
        PaintTilesAround(_bombCoordinates);

        Destroy(gameObject);
    }

    private void PaintTilesAround(Vector2Int bombCoordinates)
    {
        foreach (var offset in _offsets)
        {
            Vector2Int coords = bombCoordinates + offset;
            Tile tile = _gridManager.GetTileAtCoordinates(coords);

            if (tile == null || tile.isObstacle)
                continue;

            if (associatedPlayer != PlayerEnum.None)
            {
                PlayerEnum currentTileOwner = tile.CurrentTileOwner;

                if (currentTileOwner != associatedPlayer)
                {
                    if (currentTileOwner != PlayerEnum.None)
                    {
                        _gridManager.tilesPerPlayer[(int)currentTileOwner - 1]--;
                    }

                    _gridManager.tilesPerPlayer[(int)associatedPlayer - 1]++;
                    tile.ChangeTileColor(associatedPlayer);
                }
            }
        }
    }
}