
public class FastBomb : Bomb
{
    private float _timer2 = 1f;
    public override void ConfigureValues()
    {
        _timer2 = GameManager.Instance.RuntimeConfig.FastBombTimer;
    }

    public override float GetTimer()
    {
        return _timer2;
    }
}