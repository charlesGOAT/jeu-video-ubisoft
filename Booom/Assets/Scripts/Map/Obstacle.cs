
public class Obstacle : SpecialTile
{
    public override bool IsObstacle => true;

    public static float ObstacleHeight;

    protected override void Start()
    {
        base.Start();
        if (ObstacleHeight == 0)
        {
            ObstacleHeight = transform.GetChild(0).localScale.y;
        }
    }

    public override void StepOnTile(Player player) {}
}
