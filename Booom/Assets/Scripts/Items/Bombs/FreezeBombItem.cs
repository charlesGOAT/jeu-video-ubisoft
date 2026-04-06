
public class FreezeBombItem : BombItem
{
    public override ItemType ItemType => ItemType.FreezeBomb;
    
    protected override void PickupItemSpecific()
    {
        _associatedPlayer.SnowEffect.SetActive(true);
        _associatedPlayer.NextBombBombItems |= BombItems.FreezeBombs;
    }

    protected override void FinishUsingItemSpecific(bool hasDied = false)
    {
        _associatedPlayer.SnowEffect.SetActive(false);
        _associatedPlayer.NextBombBombItems &= ~BombItems.FreezeBombs;
    }
}
