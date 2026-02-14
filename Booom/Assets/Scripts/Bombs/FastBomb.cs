using UnityEngine;

public class FastBomb : Bomb
{
    protected override void Start()
    {
        timer = 1.0f;
        base.Start();
    }
}