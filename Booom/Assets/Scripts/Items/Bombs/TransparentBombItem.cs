
public class TransparentBombItem : BombItem
{
    protected override void UseItemSpecific()
    {
        _associatedPlayer.CreateNextBombTransparent();
    }
}
