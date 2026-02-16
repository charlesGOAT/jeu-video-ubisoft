using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItemsManager : MonoBehaviour
{
    // Manages Items for each players
    
    private readonly Dictionary<ItemType, IItem> _itemsInventory = new();
    private readonly Dictionary<ItemType, IItem> _allItems = new();

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
        IItem newItem = _allItems[item.ItemType];
        if (!_itemsInventory.TryAdd(item.ItemType, newItem)) return;
        
        newItem.PickupItem(Player);
        StartCoroutine(ManageActiveTime(newItem));
    }

    private void FinishUsingItem(IItem item)
    {
        _itemsInventory.Remove(item.ItemType);
        item.UseTimeOver(Player);
    }

    private IEnumerator ManageActiveTime(IItem item)
    {
        yield return new WaitForSeconds(item.ActiveTime);
        FinishUsingItem(item);
    }
    
    private IItem CreateItem(ItemType type)
    {
        switch (type)
        {
            case ItemType.PaintBrush:
            {
                return new PaintBrushItem();
                break;
            }
        }

        return null;
    }
}
