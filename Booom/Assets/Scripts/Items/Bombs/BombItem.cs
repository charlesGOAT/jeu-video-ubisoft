
using System;

public abstract class BombItem : BaseItem
{
    public override ItemType ItemType => ItemType.TransparentBomb;

    protected int _maxUseCount = 1;
    protected int _currentUseCount = 0;

    protected Player _associatedPlayer;

    public void UseItem()
    {
        UseItemSpecific();

        _currentUseCount++;
        if (_currentUseCount >= _maxUseCount)
        {
            FinishUsingItem();
        }
    }

    protected abstract void UseItemSpecific();

    public override void PickupItem(Player player)
    {
        _associatedPlayer = player;
        player.OnPlaceBomb += UseItem;
    }

    protected void FinishUsingItem()
    {
        _associatedPlayer.OnPlaceBomb -= UseItem;
        CallFinishUsingItemCallback();
    }
}