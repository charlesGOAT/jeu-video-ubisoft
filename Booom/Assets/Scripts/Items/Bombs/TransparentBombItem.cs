
public class TransparentBombItem : BombItem
{
    public override ItemType ItemType => ItemType.TransparentBomb;
    protected override void UseItem()
    {
        _associatedPlayer.CreateNextBombTransparent();
    }
}
