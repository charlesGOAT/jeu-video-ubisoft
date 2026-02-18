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
            PaintTilesSurrounding(_bombCoordinates, offset);
        }
    }

    private void PaintTilesSurrounding(Vector2Int bombCoordinates, Vector2Int offset)
    {
        Vector2Int coords = bombCoordinates + offset;
        
        Tile bombTile = GameManager.Instance.GridManager.GetTileAtCoordinates(bombCoordinates);
        Tile tileToPaint = GameManager.Instance.GridManager.GetTileAtCoordinates(coords);
        
        if (bombTile == null || tileToPaint == null || tileToPaint.isObstacle) return;
        
        PlayerEnum currentOwner = bombTile.CurrentTileOwner;
        PlayerEnum newTileOwner = GameManager.Instance.isSpreadingMode ? currentOwner : associatedPlayer;

        tileToPaint.ChangeTileColor(newTileOwner);

        foreach (Player player in Player.ActivePlayers)
        {
            Tile playerTile = player.GetPlayerTile();
            if (playerTile != null && playerTile.TileCoordinates == coords)
            {
                player.OnHit(coords - bombCoordinates);
            }
        }
    }
}