using System.Collections.Generic;
using UnityEngine;

public class Portal : SpecialTile
{
    [SerializeField]
    private Portal otherPortal;

    private Dictionary<PlayerEnum, bool> _hasTeleported = new Dictionary<PlayerEnum, bool>()
    {
        { PlayerEnum.Player1, false },
        { PlayerEnum.Player2, false },
        { PlayerEnum.Player3, false },
        { PlayerEnum.Player4, false }
    };

    public override bool IsObstacle => true;

    public override void StepOnTile(Player player)
    {
        if (_hasTeleported[player.PlayerNb])
        {
            return;
        }

        TeleportToOtherPortal(player);
    }

    public void TeleportToOtherPortal(Player player)
    {
        if (otherPortal == null) 
        {
            return;
        }

        otherPortal.OnTeleport(player.PlayerNb);

        player.OnPortal(otherPortal.transform.position);
    }

    public void OnTeleport(PlayerEnum playerEnum) 
    {
        _hasTeleported[playerEnum] = true;
    }

    public override void StepOffTile(Player player) 
    {
        _hasTeleported[player.PlayerNb] = false;
    }


    public Vector2Int GetOtherPortalPosition() => otherPortal.TileCoordinates;
    public Vector2 GetOtherPortalWorldPosition() => new Vector2(otherPortal.transform.position.x,otherPortal.transform.position.z);
}
