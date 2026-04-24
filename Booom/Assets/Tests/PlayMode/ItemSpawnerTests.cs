using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ItemSpawnerTests
{
    private GameObject _itemSpawnerGo;
    private ItemSpawner _itemSpawner;
    private GameObject _gameManagerGo;
    private GameObject _gridManagerGo;

    [SetUp]
    public void Setup()
    {
        Player.ActivePlayers.Clear();
        Player.PlayerColorDict.Clear();
        
        _gameManagerGo = new GameObject("GameManager");
        _gameManagerGo.AddComponent<GameManager>();
        
        _gridManagerGo = new GameObject("GridManager");
        var gridManager = _gridManagerGo.AddComponent<FakeGridManager>();
        gridManager.transform.position = Vector3.zero;
        
        _itemSpawnerGo = new GameObject("ItemSpawner");
        _itemSpawner = _itemSpawnerGo.AddComponent<ItemSpawner>();
    }

    [TearDown]
    public void Teardown()
    {
        Player.ActivePlayers.Clear();
        Player.PlayerColorDict.Clear();
        Object.Destroy(_itemSpawnerGo);
        Object.Destroy(_gridManagerGo);
        Object.Destroy(_gameManagerGo);
    }

    [Test]
    public void ItemSpawner_InitialNbItemsOnMap_IsZero()
    {
        Assert.AreEqual(0, _itemSpawner.NbItemsOnMap);
    }

    [Test]
    public void ItemSpawner_AssociatedItemType_IsPaintBrushByDefault()
    {
        Assert.AreEqual(ItemType.PaintBrush, _itemSpawner.AssociatedItemType);
    }
}