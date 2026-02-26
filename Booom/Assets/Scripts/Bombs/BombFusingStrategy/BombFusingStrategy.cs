public class BombFusingStrategy
{
    public virtual void Fuse(Bomb bomb)
    {
        bomb.StartBombCountDown();
    }
    
    public virtual void OnCollision(Bomb bomb){}
}
