
public interface IItem
{
    public ItemType ItemType => ItemType.Basic;
    public float ActiveTime => 5.0f; // basic time

    public abstract void UseItem(Player player);
    public abstract void PickupItem();

    public abstract void UseTimeOver();
}
