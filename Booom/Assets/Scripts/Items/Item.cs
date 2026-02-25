using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    private ItemType itemType = ItemType.PaintBrush;

    public ItemType ItemType => itemType;

    public Vector2Int posOnMap;
}

public enum ItemType
{
    PaintBrush = 0
}
