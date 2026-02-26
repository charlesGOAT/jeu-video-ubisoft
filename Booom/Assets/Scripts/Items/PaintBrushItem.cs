using System;
using System.Threading;
using System.Threading.Tasks;

public class PaintBrushItem : BaseItem
{
    public override ItemType ItemType => ItemType.PaintBrush;
    private const float ACTIVE_TIME = 2.5f;

    private CancellationTokenSource _cts;
    
    private Player _player;

    public void UseItem()
    {
        var gridPos = GridManagerStrategy.WorldToGridCoordinates(_player.gameObject.transform.position);
        Tile tile = GameManager.Instance.GridManager.GetTileAtCoordinates(gridPos);

        if (tile == null || tile.IsObstacle) return;
        
        tile.ChangeTileColor(_player.PlayerNb);
    }

    public override async void RepickUpItem()
    {
        _cts.Cancel();
        _cts.Dispose();
        await StartDelayTask();
    }

    public override async void PickupItem(Player player)
    {
        _player = player;
        player.OnMoveFunctionCalled += UseItem;

        await StartDelayTask();
    }

    public void UseTimeOver()
    {
        _player.OnMoveFunctionCalled -= UseItem;
        CallFinishUsingItemCallback();
    }

    private async Task StartDelayTask()
    {
        _cts = new CancellationTokenSource();
        await ManageActiveTime();
    }
    
    private async Task ManageActiveTime()
    {
        try
        {
            await Task.Delay((int)(ACTIVE_TIME * 1000), _cts.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        
        UseTimeOver();
    }
}
