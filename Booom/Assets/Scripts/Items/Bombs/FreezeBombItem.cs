
public class FreezeBombItem : BombItem
{
    public override ItemType ItemType => ItemType.FreezeBomb;
    
    protected override void PickupItemSpecific()
    {
        _associatedPlayer.ShouldNextBombFreezeBomb = true;
    }

    protected override void FinishUsingItemSpecific(bool hasDied = false)
    {
        _associatedPlayer.ShouldNextBombFreezeBomb = false;
    }
}
