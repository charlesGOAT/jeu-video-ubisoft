using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemsManager : MonoBehaviour
{
    // Manages spawning of items of one type
    // todo : make mulitple child classes for every item type

    // todo : garder un compte du nb d'items par joueur, genre l'incrémenter quand il le pickup

    protected static readonly Dictionary<ItemType, int> _itemCountPerItemTypeDict = new();
    
    public void RemoveItem(ItemType type)
    {
        if (_itemCountPerItemTypeDict[type] == 0)
            throw new Exception(
                $"There shouldn't be any items of type {type}, but you're trying to remove one.");
        
        _itemCountPerItemTypeDict[type]--;
    }
}