using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PaintBrushItem : BaseItem
{
    public override ItemType ItemType => ItemType.PaintBrush;
    private const float ACTIVE_TIME = 2.5f;

    private Dictionary<Collider, int> _modifiedBombs = new();

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
        
        GameManager.Instance.BombManager.ActivatePaintBrush(_player.gameObject.layer);
        _player.ActivatePaintbrushEffect();
        
        DisplayPopUp(player, ItemType, IconSprite);

        SoundManager.Instance.OnUsePaintBrush(true);
        await StartDelayTask();
    }

    private void UseTimeOver()
    {
        SoundManager.Instance.OnUsePaintBrush(false);
        _player.ResetPlayerTexture();
        
        GameManager.Instance.BombManager.DeactivatePaintBrush(_player.gameObject.layer);

        _player.OnMoveFunctionCalled -= UseItem;
        CallFinishUsingItemCallback();
    }

    public override void FinishUsingItem(bool hasDied = false)
    {
        _cts.Cancel();
        UseTimeOver();
    }

    private async Awaitable StartDelayTask()
    {
        _cts = new CancellationTokenSource();
        await ManageActiveTime();
    }
    
    private async Awaitable ManageActiveTime()
    {
        try
        {
            await Awaitable.WaitForSecondsAsync(ACTIVE_TIME, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        
        UseTimeOver();
    }
}
