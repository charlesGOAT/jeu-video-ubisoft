using UnityEngine;

public class PortalBomb : Bomb
{
    public int Length;
    public Vector2Int direction;
    protected override float timer => 0.0001f;

    public override void Explode() 
    {
        var bombCoordinates = _bombCoordinates + direction;
        PlayerEnum currentOwner = this.associatedPlayer;

        PlayerEnum newTileOwner = GameManager.Instance.isSpreadingMode ? currentOwner : associatedPlayer;


        for (int rangeCounter = 0; rangeCounter <= Length; ++rangeCounter)
        {
            Tile tile = GameManager.Instance.GridManager.GetTileAtCoordinates(bombCoordinates);

            if (tile == null)
            {
                break;
            }

            if (tile.IsObstacle)
            {
                if (tile is Portal portal)
                {
                    int remainingLength = Length - rangeCounter;
                    if (remainingLength > 0)
                    {
                        portal.ContinueBombExplosion(direction, remainingLength, newTileOwner);
                    }
                    break;
                }
                tile.OnExplosion(newTileOwner);
                break;
            }

            tile.OnExplosion(newTileOwner);

            foreach (Player player in Player.ActivePlayers)
            {
                Tile playerTile = player.GetPlayerTile();
                if (playerTile != null && playerTile.TileCoordinates == bombCoordinates)
                {
                    player.OnHit(direction);
                }
            }

            bombCoordinates += direction;
        }

        Destroy(gameObject);
    }
}
