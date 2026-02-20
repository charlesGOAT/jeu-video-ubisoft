
public class ChainBombsItem : BombItem
{
    private const int MAX_PLACED_BOMBS = 4; // mettre +1 la valeur qu'on veut trust

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
