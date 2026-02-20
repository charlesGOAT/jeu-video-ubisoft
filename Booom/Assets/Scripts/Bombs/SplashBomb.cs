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
        foreach (var offset in _offsets)
        {
            PaintTilesSurrounding(offset);
        }
    }

    private void PaintTilesSurrounding(Vector2Int offset)
    {
        Vector2Int coords = _bombCoordinates + offset;
        
        Tile bombTile = GameManager.Instance.GridManager.GetTileAtCoordinates(_bombCoordinates);
        Tile tileToPaint = GameManager.Instance.GridManager.GetTileAtCoordinates(coords);
        
        if (bombTile == null || tileToPaint == null || tileToPaint.isObstacle) return;
        
        PlayerEnum currentOwner = bombTile.CurrentTileOwner;
        PlayerEnum newTileOwner = GameManager.Instance.isSpreadingMode ? currentOwner : associatedPlayer;

        tileToPaint.ChangeTileColor(newTileOwner);
        
        Vector2Int hitDirection = coords - _bombCoordinates;
        HitPlayers(coords, hitDirection);
    }
}