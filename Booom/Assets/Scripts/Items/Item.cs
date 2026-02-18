using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    private ItemType itemType = ItemType.PaintBrush;

    public ItemType ItemType => itemType;
}

public enum ItemType
{
    PaintBrush = 0,
    TransparentBomb = 1
}
