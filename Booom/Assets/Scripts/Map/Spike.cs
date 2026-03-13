using UnityEngine;

public class Spike : Tile
{
    public override bool IsObstacle => true;

    protected override void Start(){}

    public override void StepOnTile(Player player)
    {
        HitPlayer(player);
    }

    public void HitPlayer(Player player)
    {
        var position = player.transform.position;
        float playerLengthToSpikeX = transform.position.x - position.x;
        float playerLengthToSpikeZ = transform.position.z - position.z;

        Vector2Int hitDir = Vector2Int.zero;

        if (Mathf.Abs(playerLengthToSpikeX) > Mathf.Abs(playerLengthToSpikeZ))
            hitDir = playerLengthToSpikeX >= 0 ? Vector2Int.left : Vector2Int.right;
        else
            hitDir = playerLengthToSpikeZ >= 0 ? Vector2Int.down : Vector2Int.up;
        
        player.OnHit(hitDir, true);
    }
}