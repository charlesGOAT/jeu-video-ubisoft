using Unity.VisualScripting;

public class Obstacle : Tile
{
    public override bool IsObstacle => true;

    public static float ObstacleHeight => 2;
}
