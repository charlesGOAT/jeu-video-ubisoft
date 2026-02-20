
public class ChainBombsItem : BombItem
{
    private const int MAX_PLACED_BOMBS = 3;

    public ChainBombsItem()
    {
        _maxUseCount = MAX_PLACED_BOMBS;
    }
    protected override void UseItemSpecific()
    {
        _associatedPlayer.isChainingBombs = true;
    }
    protected override void PickupItemSpecific()
    {
        _associatedPlayer.OnExplodeChainedBombs += FinishUsingItem;
    }

    protected override void FinishUsingItemSpecific()
    {
        _associatedPlayer.isChainingBombs = false;
    }
}
