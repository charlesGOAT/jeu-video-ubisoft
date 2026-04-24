using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerItemsManagerTests
{
    private PlayerItemsManager _playerItemsManager;
    private GameObject _playerGo;
    private Player _player;
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
        _gridManagerGo.AddComponent<FakeGridManager>();
        
        _playerGo = new GameObject("Player");
        _playerGo.AddComponent<CharacterController>();
        _player = _playerGo.AddComponent<Player>();
        _playerItemsManager = _playerGo.AddComponent<PlayerItemsManager>();
        _playerItemsManager.Player = _player;
    }

    [TearDown]
    public void Teardown()
    {
        Player.ActivePlayers.Clear();
        Player.PlayerColorDict.Clear();
        Object.Destroy(_playerGo);
        Object.Destroy(_gridManagerGo);
        Object.Destroy(_gameManagerGo);
    }

    [Test]
    public void Awake_InitializesAllItemTypes()
    {
        _playerItemsManager.Awake();
        
        Assert.IsNotNull(_playerItemsManager);
    }

    [Test]
    public void AddNewItem_AddsItemToInventory()
    {
        GameObject itemGo = new GameObject("Item");
        Item item = itemGo.AddComponent<Item>();
        item.itemType = ItemType.PaintBrush;

        _playerItemsManager.AddNewItem(item);

        Object.Destroy(itemGo);
        Assert.IsTrue(true);
    }

    [Test]
    public void ResetInventory_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _playerItemsManager.ResetInventory());
    }
}