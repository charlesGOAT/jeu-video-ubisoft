using UnityEngine;

public class GridManagerLevel : GridManagerStrategy
{
    protected override void CreateGrid()
    {
        foreach (Tile tile in FindObjectsByType<Tile>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Vector2Int tileCoordinates = WorldToGridCoordinates(tile.transform.position);

            if (_tiles.ContainsKey(tileCoordinates))
            {
                Debug.LogWarning($"Duplicate tile coordinates detected at {tileCoordinates} between '{_tiles[tileCoordinates].name}' and '{tile.name}'. Keeping the first tile.");
                continue;
            }

            _tiles[tileCoordinates] = tile;

            if (tileCoordinates.x > MapUpperLimit.x || tileCoordinates.y > MapUpperLimit.y)
            {
                MapUpperLimit = tileCoordinates;
            }

            if (tileCoordinates.x < MapLowerLimit.x || tileCoordinates.y < MapLowerLimit.y)
            {
                MapLowerLimit = tileCoordinates;
            }
        }

        Width = MapUpperLimit.x - MapLowerLimit.x + 1;
        Height = MapUpperLimit.y - MapLowerLimit.y + 1;
    }

}
