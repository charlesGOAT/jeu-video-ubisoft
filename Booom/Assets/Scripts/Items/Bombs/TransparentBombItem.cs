
public class TransparentBombItem : BombItem
{
    public override ItemType ItemType => ItemType.TransparentBomb;
    protected override void UseItem()
    {
        _associatedPlayer.ShouldNextBombBeTransparent = true;
    }
    
    protected override void FinishUsingItemSpecific()
    {
        _associatedPlayer.ShouldNextBombBeTransparent = false;
    }
}
