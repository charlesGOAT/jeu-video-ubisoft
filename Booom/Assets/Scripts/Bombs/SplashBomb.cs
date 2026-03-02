using UnityEngine;

public class SplashBomb : Bomb
{
    private readonly Vector2Int[] _offsets =
    {
        new Vector2Int(-1, -1),
        new Vector2Int(-1, -2),
        new Vector2Int(0, -1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 0),
        new Vector2Int(1, 0),
        new Vector2Int(-1, 1),
        new Vector2Int(0, 1),
        new Vector2Int(0, 2),
        new Vector2Int(1, 1),
        new Vector2Int(2, 2),
    };

    protected override void PaintTiles()
    {
        Tile bombTile = GameManager.Instance.GridManager.GetTileAtCoordinates(_bombCoordinates);

        if (bombTile == null) return;

        PlayerEnum currentOwner = bombTile.CurrentTileOwner;
        PlayerEnum newTileOwner = GameManager.Instance.IsSpreadingMode ? currentOwner : AssociatedPlayer;
        
        foreach (var offset in _offsets)
        {
            Vector2Int coords = _bombCoordinates + offset;
            PaintTileSurrounding(coords, offset, newTileOwner);
            HitPlayers(coords, offset);
        }
    }

    private void PaintTileSurrounding(Vector2Int coords, Vector2Int offset, PlayerEnum tileOwner)
    {
        Tile tileToPaint = GameManager.Instance.GridManager.GetTileAtCoordinates(coords);
        if (tileToPaint == null) return;

        if (tileToPaint is Portal portalTile)
        {
            PaintTileSurrounding(portalTile.GetOtherPortalPosition() + offset, offset, tileOwner);
            return;
        }

        if (tileToPaint.IsObstacle) return;

        tileToPaint.ChangeTileColor(tileOwner);
    }
}
