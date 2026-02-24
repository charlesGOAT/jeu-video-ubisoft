
public delegate void FinishUsingItem(BaseItem baseItem);
public abstract class BaseItem
{
    public virtual ItemType ItemType => ItemType.PaintBrush;
    public abstract void PickupItem(Player player);
    public event FinishUsingItem OnFinishUsingItem;

    protected void CallFinishUsingItemCallback()
    {
        OnFinishUsingItem?.Invoke(this);
    }
}
