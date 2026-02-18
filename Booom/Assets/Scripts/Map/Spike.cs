using UnityEngine;

public class Spike : Tile
{

    public override bool IsObstacle => true;

    public override void StepOnTile(Player player)
    {
        HitPlayer(player);
    }

    public void HitPlayer(Player player)
    {

        float playerLengthToSpikeX = transform.position.x - player.transform.position.x;
        float playerLengthToSpikeZ = transform.position.z - player.transform.position.z;

        if (Mathf.Abs(playerLengthToSpikeX) > Mathf.Abs(playerLengthToSpikeZ))
        {
            if (playerLengthToSpikeX >= 0)
            {
                player.OnHit(Vector2Int.left);
            }
            else
            {
                player.OnHit(Vector2Int.right);
            }

        }
        else
        {
            if (playerLengthToSpikeZ >= 0)
            {
                player.OnHit(Vector2Int.down);
            }
            else
            {
                player.OnHit(Vector2Int.up);
            }
        }
    }
}