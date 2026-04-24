using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ItemsManagerTests
{
    private ItemsManager _itemsManager;
    private GameObject _itemsManagerGo;
    private GameObject _gameManagerGo;
    private GameObject _itemSpawnerGo;
    private ItemSpawner _itemSpawner;

    [SetUp]
    public void Setup()
    {
        Player.ActivePlayers.Clear();
        Player.PlayerColorDict.Clear();
        
        _gameManagerGo = new GameObject("GameManager");
        _gameManagerGo.AddComponent<GameManager>();
        
        _itemsManagerGo = new GameObject("ItemsManager");
        _itemsManager = _itemsManagerGo.AddComponent<ItemsManager>();
        
        _itemSpawnerGo = new GameObject("ItemSpawner");
        _itemSpawner = _itemSpawnerGo.AddComponent<ItemSpawner>();
    }

    [TearDown]
    public void Teardown()
    {
        Player.ActivePlayers.Clear();
        Player.PlayerColorDict.Clear();
        Object.Destroy(_itemSpawnerGo);
        Object.Destroy(_itemsManagerGo);
        Object.Destroy(_gameManagerGo);
    }

    [Test]
    public void RemoveItem_DecrementsItemCount()
    {
        _itemSpawner.NbItemsOnMap = 1;
        
        _itemsManager.RemoveItem(ItemType.PaintBrush);
        
        Assert.AreEqual(0, _itemSpawner.NbItemsOnMap);
    }

    [Test]
    public void RemoveItem_ThrowsWhenNoItemsToRemove()
    {
        _itemSpawner.NbItemsOnMap = 0;
        
        Assert.Throws<System.Exception>(() => _itemsManager.RemoveItem(ItemType.PaintBrush));
    }

    [Test]
    public void StartSpawning_DoesNotThrowInEditor()
    {
        Assert.DoesNotThrow(() => _itemsManager.StartSpawning());
    }
}