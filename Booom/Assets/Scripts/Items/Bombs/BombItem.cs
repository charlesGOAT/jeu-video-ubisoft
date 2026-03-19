
public abstract class BombItem : BaseItem
{
    protected virtual int maxUseCount => 1;
    private int _currentUseCount = 0;

    protected Player _associatedPlayer;

    protected void BombSuccessfullyPlaced()
    {
        _currentUseCount++;
        if (_currentUseCount >= maxUseCount)
        {
            FinishUsingItem();
        }
    }

    protected virtual void UseItem() {}

    public override void PickupItem(Player player)
    {
        _associatedPlayer = player;
        _associatedPlayer.OnPlaceBomb += UseItem;
        _associatedPlayer.OnPlaceBombSuccessful += BombSuccessfullyPlaced;
        
        PickupItemSpecific();
        _associatedPlayer.DisplayPopUp(ItemType, IconSprite);
    }

    protected virtual void PickupItemSpecific() {}

    public override void FinishUsingItem(bool hasDied = false)
    {
        _associatedPlayer.OnPlaceBomb -= UseItem;
        _associatedPlayer.OnPlaceBombSuccessful -= BombSuccessfullyPlaced;
        _associatedPlayer.RemoveItemPopUp();
        
        _currentUseCount = 0;
        FinishUsingItemSpecific(hasDied);
        CallFinishUsingItemCallback();
    }

    protected abstract void FinishUsingItemSpecific(bool hasDied = false);
}