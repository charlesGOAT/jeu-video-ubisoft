using UnityEngine;

public class GridManagerLevel : GridManagerStrategy
{
    protected override void CreateGrid()
    {
        foreach (Tile tile in FindObjectsByType<Tile>(FindObjectsSortMode.None))
        {
            _tiles[tile.TileCoordinates] = tile;

            if (tile.TileCoordinates.x > MapUpperLimit.x || tile.TileCoordinates.y > MapUpperLimit.y)
            {
                MapUpperLimit = tile.TileCoordinates;
            }

            if (tile.TileCoordinates.x < MapLowerLimit.x || tile.TileCoordinates.y < MapLowerLimit.y)
            {
                MapLowerLimit = tile.TileCoordinates;
            }
        }

        Width = MapUpperLimit.x - MapLowerLimit.x + 1;
        Height = MapUpperLimit.y - MapLowerLimit.y + 1;
    }

}
