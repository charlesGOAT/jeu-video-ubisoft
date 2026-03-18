using UnityEngine;

public class Portal : SpecialTile
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
        
        Vector2Int portalDir = Vector2Int.zero;

        if (Mathf.Abs(playerLengthToPortalX) > Mathf.Abs(playerLengthToPortalZ))
            portalDir = playerLengthToPortalX >= 0 ? Vector2Int.right : Vector2Int.left;
        else
            portalDir = playerLengthToPortalZ >= 0 ? Vector2Int.up : Vector2Int.down;
        
        player.OnPortal(portalDir, otherPortal.transform.position);
    }

    public Vector2Int GetOtherPortalPosition() => otherPortal.TileCoordinates;
    public Vector2 GetOtherPortalWorldPosition() => new Vector2(otherPortal.transform.position.x,otherPortal.transform.position.z);
}
