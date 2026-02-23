using Unity.VisualScripting;

public class Obstacle : Tile
{
    public override bool IsObstacle => true;

    public static float ObstacleHeight;

    protected override void Awake()
    {
        base.Awake();
        if (ObstacleHeight == 0)
        {
            ObstacleHeight = transform.GetChild(0).localScale.y;
        }
    }
}
