using UnityEngine;

public class Portal : Tile
{
    [SerializeField]
    private Portal otherPortal; 

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
                player.OnPortal(Vector2Int.right, otherPortal.transform.position);
            }
            else
            {
                player.OnPortal(Vector2Int.left, otherPortal.transform.position);
            }
        }
        else
        {
            if (playerLengthToSpikeZ >= 0)
            {
                player.OnPortal(Vector2Int.up, otherPortal.transform.position);
            }
            else
            {
                player.OnPortal(Vector2Int.down, otherPortal.transform.position);
            }
        }
    }

    public void ContinueBombExplosion(Vector2Int direction, int length, PlayerEnum player) 
    {
        var portalBombPrefab = GameManager.Instance.BombManager.bombPrefabs[(int)BombEnum.PortalBomb] as PortalBomb;
        if (portalBombPrefab == null)
        {
            return;
        }

        PortalBomb portalBombInstance = Instantiate(portalBombPrefab, otherPortal.transform.position, Quaternion.identity);
        portalBombInstance.associatedPlayer = player;
        portalBombInstance.Length = length;
        portalBombInstance.direction = direction;

    }

    public Vector2Int GetOtherPortalPosition() => otherPortal.TileCoordinates; 
}
