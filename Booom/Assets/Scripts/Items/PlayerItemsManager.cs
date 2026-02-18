using System;
using System.Collections.Generic;
using UnityEngine;



public class PlayerItemsManager : MonoBehaviour
{
    // Manages Items for each players
    
    private readonly Dictionary<ItemType, BaseItem> _itemsInventory = new();
    private readonly Dictionary<ItemType, BaseItem> _allItems = new();

    public Player Player { private get; set; }
    

    private void Awake()
    {
        foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
        {
            _allItems[itemType] = CreateItem(itemType);
        }
    }

    public void AddNewItem(Item item)
    {
        BaseItem newBaseItem = _allItems[item.ItemType];
        if (!_itemsInventory.TryAdd(item.ItemType, newBaseItem)) return;
        
        newBaseItem.PickupItem(Player);
        newBaseItem.OnFinishUsingItem += FinishUsingItem;
    }

    private void FinishUsingItem(BaseItem baseItem)
    {
        _itemsInventory.Remove(baseItem.ItemType);
    }

    private BaseItem CreateItem(ItemType type)
    {
        switch (type)
        {
            case ItemType.PaintBrush:
            {
                return new PaintBrushItem();
            }
            case ItemType.TransparentBomb:
            {
                return new TransparentBombItem();
            }
            // todo : add more
        }

        return null;
    }
}
