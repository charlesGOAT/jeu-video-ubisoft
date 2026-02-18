using UnityEngine;

public class Trampoline : Tile
{

    public override bool IsObstacle => true;

    public override void StepOnTile(Player player)
    {
        UseTrampoline(player);
    }

    public void UseTrampoline(Player player)
    {
        float playerLengthToTrampolineX = transform.position.x - player.transform.position.x;
        float playerLengthToTrampolineZ = transform.position.z - player.transform.position.z;

        if (Mathf.Abs(playerLengthToTrampolineX) > Mathf.Abs(playerLengthToTrampolineZ))
        {
            if (playerLengthToTrampolineX >= 0)
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
            if (playerLengthToTrampolineZ >= 0)
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