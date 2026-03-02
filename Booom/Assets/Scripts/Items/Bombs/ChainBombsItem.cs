
public class ChainBombsItem : BombItem
{
    public override ItemType ItemType => ItemType.ChainBombs;
    protected override int maxUseCount => 3;
    
    protected override void PickupItemSpecific()
    {
        _associatedPlayer.OnExplodeChainedBombs += FinishUsingChainedBombs;
        _associatedPlayer.BombFusingType = BombFusingType.Chained;
    }

    protected override void FinishUsingItemSpecific(bool hasDied = false)
    {
        _associatedPlayer.OnExplodeChainedBombs -= FinishUsingChainedBombs;
        _associatedPlayer.BombFusingType = BombFusingType.None;
        
        if(hasDied)
            GameManager.Instance.BombManager.ExplodeChainedBombs(_associatedPlayer.PlayerNb);
    }

    private void FinishUsingChainedBombs()
    {
        FinishUsingItem();
    }
}
