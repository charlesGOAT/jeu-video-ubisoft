using UnityEngine;

public class Obstacle : Tile {
    public static float ObstacleHeight { get; private set; }

    public override void OnExplosion(PlayerEnum newOwner)
    {
       
    }

    void Awake()
    {
        ObstacleHeight = transform.GetChild(0).gameObject.transform.localScale.y;
    }
}
