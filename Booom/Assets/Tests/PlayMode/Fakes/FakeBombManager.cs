using UnityEngine;

public class FakeBombManager : BombManager
{
    public bool bombCreated;

    protected override void Awake() {}

    public override void CreateBomb(Vector3 position, PlayerEnum playerEnum, BombEnum bombEnum, bool isTransparentBomb = false, bool isChained = false)
    {
        bombCreated = true;
    }
}