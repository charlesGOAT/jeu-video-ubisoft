
public class FreezeBombItem : BombItem
{
    public override ItemType ItemType => ItemType.FreezeBomb;
    
    protected override void PickupItemSpecific()
    {
        _associatedPlayer.NextBombBombItems |= BombItems.FreezeBombs;
    }

    protected override void FinishUsingItemSpecific(bool hasDied = true)
    {
        _associatedPlayer.NextBombBombItems &= ~BombItems.FreezeBombs;
    }
}
