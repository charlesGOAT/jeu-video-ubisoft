
public class TargetBombItem : BombItem
{
    public override ItemType ItemType => ItemType.TargetBomb;
    
    protected override void UseItem()
    {
        if (!_associatedPlayer.BombFusingType.Equals(BombFusingType.Chained))
            _associatedPlayer.BombFusingType = BombFusingType.Target;
    }
    
    protected override void FinishUsingItemSpecific()
    {
        _associatedPlayer.BombFusingType = BombFusingType.None;
    }
}
