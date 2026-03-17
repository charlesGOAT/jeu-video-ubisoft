
public class ChainBombsItem : BombItem
{
    public override ItemType ItemType => ItemType.ChainBombs;
    protected override int maxUseCount => 2; // actual use count = maxUseCount + 1
    
    protected override void PickupItemSpecific()
    {
        _associatedPlayer.NextBombBombItems |= BombItems.ChainedBombs;
    }

    protected override void FinishUsingItemSpecific(bool hasDied = false)
    {
        _associatedPlayer.NextBombBombItems &= ~BombItems.ChainedBombs;
    }
}
