using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public delegate void FinishUsingItem(BaseItem baseItem);
public abstract class BaseItem
{
    public Sprite IconSprite;
    public virtual ItemType ItemType => ItemType.PaintBrush;
    public abstract void PickupItem(Player player);
    public event FinishUsingItem OnFinishUsingItem;
    
    public virtual void RepickUpItem(){}

    public abstract void FinishUsingItem(bool hasDied = false);

    protected void CallFinishUsingItemCallback()
    {
        OnFinishUsingItem?.Invoke(this);
    }
}
