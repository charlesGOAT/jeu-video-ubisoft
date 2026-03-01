using UnityEngine;

public class FakeBombManager : BombManager
{
    public bool bombCreated;

    protected override void Awake() {}

    public override bool CreateBomb(Vector3 position, PlayerEnum playerEnum, bool isTransparentBomb = false, bool isChained = false)
    {
        bombCreated = true;
        return true;
    }
}