
public class FastBomb : Bomb
{
    public override float Timer { get; protected set; } = 1f;
    public override void ConfigureValues()
    {
        Timer = GameManager.Instance.RuntimeConfig.FastBombTimer;
    }
}