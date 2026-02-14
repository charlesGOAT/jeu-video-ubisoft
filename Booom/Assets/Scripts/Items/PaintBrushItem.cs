
public class PaintBrushItem : IItem
{
    public ItemType ItemType => ItemType.PaintBrush;
    public float ActiveTime => 1.5f;

    public void UseItem(Player player)
    {
        var gridPos = GridManagerStategy.WorldToGridCoordinates(player.gameObject.transform.position);
        Tile tile = GameManager.Instance.GridManager.GetTileAtCoordinates(gridPos);

        if (tile == null || tile.isObstacle) return;
        
        tile.ChangeTileColor(player.PlayerNb);
    }

    public void PickupItem(Player player)
    {
        player.OnMoveFunctionCalled += UseItem;
    }

    public void UseTimeOver(Player player)
    {
        player.OnMoveFunctionCalled -= UseItem;
    }
}
