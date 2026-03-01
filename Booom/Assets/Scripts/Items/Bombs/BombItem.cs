
public abstract class BombItem : BaseItem
{
    protected virtual int maxUseCount => 1;
    private int _currentUseCount = 0;

    protected Player _associatedPlayer;

    protected void BombPlacedExploded()
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
        _associatedPlayer.OnBombExploded += BombPlacedExploded;
        
        PickupItemSpecific();
    }
    
    protected virtual void PickupItemSpecific() {}

    protected void FinishUsingItem()
    {
        _associatedPlayer.OnPlaceBomb -= UseItem;
        _associatedPlayer.OnBombExploded -= BombPlacedExploded;
        _currentUseCount = 0;
        FinishUsingItemSpecific();
        CallFinishUsingItemCallback();
    }
    
    protected virtual void FinishUsingItemSpecific() {}
}