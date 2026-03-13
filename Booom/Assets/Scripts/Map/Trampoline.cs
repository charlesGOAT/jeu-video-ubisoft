using UnityEngine;

public class Trampoline : Tile
{
    public override bool IsObstacle => true;

    protected override void Start(){}

    public override void StepOnTile(Player player)
    {
        UseTrampoline(player);
    }

    public void UseTrampoline(Player player)
    {
        float playerLengthToTrampolineX = transform.position.x - player.transform.position.x;
        float playerLengthToTrampolineZ = transform.position.z - player.transform.position.z;
        
        Vector2Int jumpDir = Vector2Int.zero;
        
        if (Mathf.Abs(playerLengthToTrampolineX) > Mathf.Abs(playerLengthToTrampolineZ))
            jumpDir = playerLengthToTrampolineX >= 0 ? Vector2Int.right : Vector2Int.left;
        else
            jumpDir = playerLengthToTrampolineZ >= 0 ? Vector2Int.up : Vector2Int.down;
        
        player.OnJump(jumpDir);
    }
}