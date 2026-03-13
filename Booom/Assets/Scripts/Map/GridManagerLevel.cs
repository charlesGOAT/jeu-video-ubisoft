using UnityEngine;

public class GridManagerLevel : GridManagerStrategy
{
    //Faudra fix lowkey c une source d'erreur
    protected override void CreateGrid()
    {
        foreach (Tile tile in FindObjectsByType<Tile>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
<<<<<<< HEAD
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
=======
            tile.InitializeTileCoordinates();
            Vector2Int tileCoordinates = tile.TileCoordinates;
            _tiles[tileCoordinates] = tile;

            if (tileCoordinates.x > MapUpperLimit.x || tileCoordinates.y > MapUpperLimit.y)
            {
                MapUpperLimit = tileCoordinates;
            }

            if (tileCoordinates.x < MapLowerLimit.x || tileCoordinates.y < MapLowerLimit.y)
            {
>>>>>>> main
                MapLowerLimit = tileCoordinates;
            }
        }

        Width = MapUpperLimit.x - MapLowerLimit.x + 1;
        Height = MapUpperLimit.y - MapLowerLimit.y + 1;
    }
}
