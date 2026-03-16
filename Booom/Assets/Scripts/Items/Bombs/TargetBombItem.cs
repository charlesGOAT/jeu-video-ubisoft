
public class TargetBombItem : BombItem
{
    public override ItemType ItemType => ItemType.TargetBomb;
    
    protected override void UseItem()
    {
        _associatedPlayer.BombFusingType = BombFusingType.Target;
    }
    
    protected override void FinishUsingItemSpecific(bool hasDied = false)
    {
        _associatedPlayer.BombFusingType = BombFusingType.None;
    }
}
