using UnityEngine;

public class FakeBombManager : BombManager
{
    public bool bombCreated;

    protected override void Awake() {}

<<<<<<< HEAD
    public override bool CreateBomb(Vector3 position, PlayerEnum playerEnum, BombEnum bombEnum, bool isTransparentBomb = false, bool isChained = false)
=======

    public override bool CreateBomb(in Vector3 position, in Player player, in BombFusingStrategy bombStrat, bool isTransparentBomb = false, bool _ = false)
>>>>>>> main
    {
        bombCreated = true;
        return true;
    }
}