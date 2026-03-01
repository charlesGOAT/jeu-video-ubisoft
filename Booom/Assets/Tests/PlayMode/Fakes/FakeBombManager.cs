using UnityEngine;

public class FakeBombManager : BombManager
{
    public bool bombCreated;

    protected override void Awake() {}

    public override bool CreateBomb(Vector3 position, PlayerEnum playerEnum, BombEnum bombEnum, BombFusingStrategy bombStrat, bool isTransparentBomb = false)
    {
        bombCreated = true;
        return true;
    }
}