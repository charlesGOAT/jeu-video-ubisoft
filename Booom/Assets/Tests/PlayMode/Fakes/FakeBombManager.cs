using UnityEngine;

public class FakeBombManager : BombManager
{
    public bool bombCreated;

    protected override void Awake() {}

    public override bool CreateBomb(in Vector3 position, in Player player, in BombFusingStrategy bombStrat, bool isTransparentBomb = false)
    {
        bombCreated = true;
        return true;
    }
}