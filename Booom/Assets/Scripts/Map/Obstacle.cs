public class Obstacle : SpecialTile
{
    public override bool IsObstacle => true;

    public static float ObstacleHeight = 1.428571f;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void StepOnTile(Player player) {}
}
