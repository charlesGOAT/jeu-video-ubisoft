using UnityEngine;

public class FakeBombManager : BombManager
{
    public bool bombCreated;

    protected override void Awake() {}


    public override bool CreateBomb(in Vector3 position, in PlayerEnum player, in BombFusingStrategy bombStrat, in BombItems _)
    {
        bombCreated = true;
        return true;
    }
}