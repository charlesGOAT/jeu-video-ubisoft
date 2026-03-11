using UnityEngine;

public class FakeBombManager : BombManager
{
    public bool bombCreated;

    protected override void Awake() {}

    public override bool CreateBomb(Vector3 position, PlayerEnum playerEnum, BombFusingStrategy bombStrat, bool isTransparentBomb = false, bool _ = false)
    {
        bombCreated = true;
        return true;
    }
}