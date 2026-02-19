using Unity.VisualScripting;

public class Obstacle : Tile
{
    public override bool IsObstacle => true;

    public static float ObstacleHeight;

    private void Awake()
    {
        base.Awake();
        if (ObstacleHeight == 0)
        {
            TileLength = transform.GetChild(0).localScale.y;
        }
    }
}
