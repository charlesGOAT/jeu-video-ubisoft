
public class ChainBombsItem : BombItem
{
    public override ItemType ItemType => ItemType.ChainBombs;
    protected override int maxUseCount => 3;
    
    protected override void PickupItemSpecific()
    {
        _associatedPlayer.OnExplodeChainedBombs += FinishUsingItem;
        _associatedPlayer.BombFusingType = BombFusingType.Chained;
    }

    protected override void FinishUsingItemSpecific()
    {
        _associatedPlayer.OnExplodeChainedBombs -= FinishUsingItem;
        _associatedPlayer.BombFusingType = BombFusingType.None;
    }
}
