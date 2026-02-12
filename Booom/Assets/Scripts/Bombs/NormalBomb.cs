using UnityEngine;

public class NormalBomb : Bomb
{
    [SerializeField]
    private int explosionRange = 3;

    private readonly Vector2Int[] _directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    protected override void Explode()
    {
        foreach (Vector2Int direction in _directions)
        {
            PaintTilesForDirection(_bombCoordinates, direction);
        }

        Destroy(gameObject);
    }

    private void PaintTilesForDirection(Vector2Int bombCoordinates, Vector2Int direction)
    {
        for (int rangeCounter = 0; rangeCounter <= explosionRange; ++rangeCounter)
        {
            Tile tile = _gridManager.GetTileAtCoordinates(bombCoordinates);

            if (tile == null || tile.isObstacle)
                return;

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

            bombCoordinates += direction;
        }
    }
}