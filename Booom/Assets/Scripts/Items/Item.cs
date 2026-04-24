using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    public ItemType itemType = ItemType.PaintBrush;
    [SerializeField]
    public Sprite iconSprite;

    public ItemType ItemType => itemType;

    public Vector2Int posOnMap;
}

public enum ItemType
{
    PaintBrush = 0,
    ChainBombs = 1,
    TargetBomb = 2,
    FreezeBomb = 3
}
