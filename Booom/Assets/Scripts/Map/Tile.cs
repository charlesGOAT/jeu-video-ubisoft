using UnityEngine;

public abstract class Tile : MonoBehaviour
{
    public Vector2Int TileCoordinates { get; private set; }
    public static float TileLength { get; private set; }
    public virtual bool IsObstacle => true;

    protected void Awake()
    {
        TileLength = transform.GetChild(0).localScale.x;
        TileCoordinates = GridManagerStategy.WorldToGridCoordinates(transform.position);
    }

    public abstract void OnExplosion(PlayerEnum newOwner);
}
