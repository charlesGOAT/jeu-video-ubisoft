using UnityEngine;

public class SplashBomb : Bomb
{
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
        foreach (var offset in _offsets)
        {
            PaintTilesForDirection(_bombCoordinates, offset);
        }

        Destroy(gameObject);
    }

    protected override void PaintTilesForDirection(Vector2Int bombCoordinates, Vector2Int offset)
    {
        Vector2Int coords = bombCoordinates + offset;
        Tile tile = _gridManager.GetTileAtCoordinates(coords);

        if (tile == null || tile.isObstacle) return;
        
        tile.ChangeTileColor(associatedPlayer);
    }
}