using UnityEngine;

public class SpecialTile : Tile
{
    protected override void Start(){}

    public override void ChangeTileColor(PlayerEnum newOwner) { }

    protected override void ChangeTileMaterial(int matIndex, in Material mat){}
}
