using UnityEngine;

public class Portal : Tile
{
    [SerializeField]
    private Portal otherPortal;

    public override bool IsObstacle => true;

    public override void StepOnTile(Player player)
    {
        TeleportToOtherPortal(player);
    }

    public void TeleportToOtherPortal(Player player)
    {
        if (otherPortal == null) 
        {
            return;
        }

        float playerLengthToPortalX = transform.position.x - player.transform.position.x;
        float playerLengthToPortalZ = transform.position.z - player.transform.position.z;

        if (Mathf.Abs(playerLengthToPortalX) > Mathf.Abs(playerLengthToPortalZ))
        {
            if (playerLengthToPortalX >= 0)
            {
                player.OnPortal(Vector2Int.right, otherPortal.transform.position);
            }
            else
            {
                player.OnPortal(Vector2Int.left, otherPortal.transform.position);
            }
        }
        else
        {
            if (playerLengthToPortalZ >= 0)
            {
                player.OnPortal(Vector2Int.up, otherPortal.transform.position);
            }
            else
            {
                player.OnPortal(Vector2Int.down, otherPortal.transform.position);
            }
        }
    }

    public Vector2Int GetOtherPortalPosition() => otherPortal.TileCoordinates;
    public Vector2 GetOtherPortalWorldPosition() => new Vector2(otherPortal.transform.position.x,otherPortal.transform.position.z);
}
