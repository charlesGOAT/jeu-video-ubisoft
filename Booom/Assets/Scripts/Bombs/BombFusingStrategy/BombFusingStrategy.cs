public class BombFusingStrategy
{
    public virtual void Fuse(Bomb bomb)
    {
        SoundManager.Instance.OnBombFused();
        bomb.StartBombCountDown();
    }

    public virtual void OnCollision(in Bomb bomb){}
}
