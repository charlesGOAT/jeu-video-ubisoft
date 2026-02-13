using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    private ItemType itemType = ItemType.Basic;

    public ItemType ItemType => itemType;
}

public enum ItemType
{
    Basic = 0, // todo add more
    PaintBrush = 1
}
