using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItemsManager : MonoBehaviour
{
    // Manages Items for each players
    
    private readonly Dictionary<ItemType, BaseItem> _itemsInventory = new();
    private readonly Dictionary<ItemType, BaseItem> _allItems = new();

    public Player Player { private get; set; }
    
    private void Start()
    {
        foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
        {
            _allItems[itemType] = CreateItem(itemType);
        }
    }

    public void AddNewItem(Item item)
    {
        BaseItem newBaseItem = _allItems[item.ItemType];
        newBaseItem.IconSprite = item.iconSprite;
        if (!_itemsInventory.TryAdd(item.ItemType, newBaseItem))
        {
            _itemsInventory[item.ItemType].RepickUpItem();
            return;
        }
        
        newBaseItem.PickupItem(Player);
        newBaseItem.OnFinishUsingItem += FinishUsingItem;
    }

    private void FinishUsingItem(BaseItem baseItem)
    {
        _itemsInventory.Remove(baseItem.ItemType);
    }

    public void ResetInventory()
    {
        foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
        {
            if(_itemsInventory.TryGetValue(itemType, out BaseItem item))
                item.FinishUsingItem(true);
        }
    }

    private BaseItem CreateItem(ItemType type)
    {
        switch (type)
        {
            case ItemType.PaintBrush:
                return new PaintBrushItem();
            case ItemType.ChainBombs:
                return new ChainBombsItem();
            case ItemType.TargetBomb:
                return new TargetBombItem();
            case ItemType.FreezeBomb:
                return new FreezeBombItem();
            
            // todo : add more
        }

        return null;
    }
}
