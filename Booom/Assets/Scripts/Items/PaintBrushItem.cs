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

    public override async void RepickUpItem()
    {
        _cts.Cancel();
        _cts.Dispose();
        await StartDelayTask();
    }

    public override async void PickupItem(Player player)
    {
        _player = player;
        GameManager.Instance.BombManager.ActivatePaintBrush(_player.gameObject.layer);
        _player.ActivatePaintbrushEffect();
        _player.IsUsingPaintbrush = true;
        player.CurrentTile.ChangeTileColor(player.PlayerNb);

        player.DisplayPopUp(ItemType, IconSprite);

        SoundManager.Instance.OnUsePaintBrush(true);
        await StartDelayTask();
    }

    private void UseTimeOver()
    {
        SoundManager.Instance.OnUsePaintBrush(true);
        _player.ResetPlayerTexture();
        _player.IsUsingPaintbrush = true;
        
        GameManager.Instance.BombManager.DeactivatePaintBrush(_player.gameObject.layer);

        CallFinishUsingItemCallback();
    }

    public override void FinishUsingItem(bool hasDied = true)
    {
        _player.RemoveItemPopUp(ItemType);
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
        
        FinishUsingItem();
    }
}
