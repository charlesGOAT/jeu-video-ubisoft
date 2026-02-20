using UnityEngine;

public class FakeGridManager : GridManagerStrategy
{
    protected override void CreateGrid() {}

    public override Tile GetTileAtCoordinates(Vector2Int vector2Int)
    {
        GameObject tileGo = new GameObject();
        var fakeTile = tileGo.AddComponent<FakeTile>();
        
        return fakeTile;
    }
}
