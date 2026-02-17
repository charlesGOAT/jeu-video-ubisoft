using System;

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SpawnMode
{
    Fixed = 0,
    Random = 1,
    Strategic = 2
}

[Serializable]
public struct FixedPos
{
    public List<Vector2Int> fixedPosList;
}

[RequireComponent(typeof(ItemSpawner))]
public class ItemsManager : MonoBehaviour
{
    [SerializeField] 
    public SpawnMode spawnMode = SpawnMode.Fixed;

    private readonly ItemSpawner[] _itemSpawnerPerItemType = new ItemSpawner[Enum.GetValues(typeof(ItemType)).Length];
    
    [SerializeField]
    private bool isDropFromSky = false;

    public void RemoveItem(ItemType type)
    {
        if (_itemSpawnerPerItemType[(int)type].NbItemsOnMap == 0)
            throw new Exception(
                $"There shouldn't be any items of type {nameof(type)}, but you're trying to remove one.");

        _itemSpawnerPerItemType[(int)type].NbItemsOnMap--;
    }

    private void Awake()
    {
        InitialiseSpawners();
        StartSpawning();
    }

    private void InitialiseSpawners()
    {
        var spawners = GetComponents<ItemSpawner>();
        
        foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
        {
            _itemSpawnerPerItemType[(int)type] = spawners.First(spawner => spawner.AssociatedItemType == type);
        }
    }

    private void StartSpawning()
    {
        foreach (var itemSpawner in _itemSpawnerPerItemType)
        {
            itemSpawner.Spawn(spawnMode, isDropFromSky);
        }
    }
}