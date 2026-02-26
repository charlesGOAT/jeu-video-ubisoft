public class BombFusingStrategy
{
    public virtual void Fuse(Bomb bomb)
    {
        bomb.StartBombCountDown();
    }
}
