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

    protected override void PaintTiles()
    {
        Tile bombTile = GameManager.Instance.GridManager.GetTileAtCoordinates(_bombCoordinates);

        if (bombTile == null) return;

        PlayerEnum currentOwner = bombTile.CurrentTileOwner;
        PlayerEnum newTileOwner = GameManager.Instance.isSpreadingMode ? currentOwner : AssociatedPlayer;

        foreach (var offset in _offsets)
        {
            PaintTilesSurrounding(_bombCoordinates, offset, newTileOwner);
        }
    }

    private void PaintTilesSurrounding(Vector2Int bombCoordinates, Vector2Int offset, PlayerEnum tileOwner)
    {
        Vector2Int coords = bombCoordinates + offset;
        Tile tileToPaint = GameManager.Instance.GridManager.GetTileAtCoordinates(coords);
        if (tileToPaint == null) return;

        if (tileToPaint is Portal portalTile)
        {
            PaintTilesSurrounding(portalTile.GetOtherPortalPosition(), offset, tileOwner);
            return;
        }

        if (tileToPaint.IsObstacle) return;

        tileToPaint.ChangeTileColor(tileOwner);

        Vector2Int hitDirection = coords - _bombCoordinates;
        HitPlayers(coords, hitDirection);
    }
}
