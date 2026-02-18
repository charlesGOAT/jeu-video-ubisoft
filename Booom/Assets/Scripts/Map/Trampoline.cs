using UnityEngine;

public class Trampoline : Tile
{
    public override void OnExplosion(PlayerEnum newOwner)
    {
    }

    public void Bruh(Player player)
    {
        float playerLengthToSpikeX = transform.position.x - player.transform.position.x;
        float playerLengthToSpikeZ = transform.position.z - player.transform.position.z;

        if (Mathf.Abs(playerLengthToSpikeX) > Mathf.Abs(playerLengthToSpikeZ))
        {
            if (playerLengthToSpikeX >= 0)
            {
                player.OnJump(Vector2Int.right);
            }
            else
            {
                player.OnJump(Vector2Int.left);
            }
        }
        else
        {
            if (playerLengthToSpikeZ >= 0)
            {
                player.OnJump(Vector2Int.up);
            }
            else
            {
                player.OnJump(Vector2Int.down);
            }
        }
    }
}
