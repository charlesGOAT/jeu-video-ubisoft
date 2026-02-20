
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class PaintBrushItem : BaseItem
{
    public override ItemType ItemType => ItemType.PaintBrush;
    private const float ACTIVE_TIME = 2.5f;
    
    private Player _player;

    public void UseItem()
    {
        var gridPos = GridManagerStategy.WorldToGridCoordinates(_player.gameObject.transform.position);
        Tile tile = GameManager.Instance.GridManager.GetTileAtCoordinates(gridPos);

        if (tile == null || tile.isObstacle) return;
        
        tile.ChangeTileColor(_player.PlayerNb);
    }

    public override async void PickupItem(Player player)
    {
        _player = player;
        player.OnMoveFunctionCalled += UseItem;
        await ManageActiveTime();
    }

    public void UseTimeOver()
    {
        _player.OnMoveFunctionCalled -= UseItem;
        CallFinishUsingItemCallback();
    }
    
    private async Task ManageActiveTime()
    {
        await Task.Delay((int)(ACTIVE_TIME * 1000));
        UseTimeOver();
    }
}
