
public class FastBomb : Bomb
{
    public override float Timer { get; protected set; } = 1f;
    protected override void ConfigureValues()
    {
        Timer = GameManager.Instance.RuntimeConfig.FastBombTimer;
    }
}