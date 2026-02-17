
public interface IItem
{
    public ItemType ItemType => ItemType.PaintBrush;
    public float ActiveTime => 5.0f; // basic time

    public void UseItem(Player player);
    public void PickupItem(Player player);

    public void UseTimeOver(Player player);
}
