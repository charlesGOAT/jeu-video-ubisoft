
using System.Threading;

public class TargetBombItem : BombItem
{
    public override ItemType ItemType => ItemType.TargetBomb;
    
    protected override void UseItem()
    {
        if (!_associatedPlayer.BombFusingType.Equals(BombFusingType.Chained))
        {
            _associatedPlayer.BombFusingType = BombFusingType.Target;
            _associatedPlayer.OnBombExploded += BombPlacedExploded;
        }
        else
        {
            _associatedPlayer.OnBombExploded -= BombPlacedExploded; // ensures it's not triggered by chained bombs exploding
        }
    }
    
    protected override void FinishUsingItemSpecific()
    {
        _associatedPlayer.BombFusingType = BombFusingType.None;
    }
}
