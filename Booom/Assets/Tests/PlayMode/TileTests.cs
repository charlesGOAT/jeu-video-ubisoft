using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TileTests
{
    private GameObject _tileGo;
    private FakeTile _tile;
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
        
        _tileGo = new GameObject("Tile");
        _tile = _tileGo.AddComponent<FakeTile>();
    }

    [TearDown]
    public void Teardown()
    {
        Player.ActivePlayers.Clear();
        Player.PlayerColorDict.Clear();
        Object.Destroy(_tileGo);
        Object.Destroy(_gridManagerGo);
        Object.Destroy(_gameManagerGo);
    }

    [Test]
    public void Tile_InitializesCoordinates()
    {
        _tile.InitializeTileCoordinates();
        Assert.AreNotEqual(Vector2Int.zero, _tile.TileCoordinates);
    }

    [Test]
    public void Tile_IsNotObstacleByDefault()
    {
        Assert.IsFalse(_tile.IsObstacle);
    }

    [Test]
    public void Tile_IsFrozen_FalseByDefault()
    {
        Assert.IsFalse(_tile.IsFrozen);
    }

    [Test]
    public void Tile_CurrentTileOwner_IsNoneByDefault()
    {
        Assert.AreEqual(PlayerEnum.None, _tile.CurrentTileOwner);
    }

    [Test]
    public void ClampColor_ClampsValuesBetween0And1()
    {
        Color overSaturated = new Color(1.5f, -0.5f, 2f, 0.5f);
        Color clamped = _tile.ClampColor(overSaturated);

        Assert.AreEqual(1f, clamped.r);
        Assert.AreEqual(0f, clamped.g);
        Assert.AreEqual(1f, clamped.b);
        Assert.AreEqual(0.5f, clamped.a);
    }

    [Test]
    public void ClampColor_PreservesValidValues()
    {
        Color valid = new Color(0.5f, 0.5f, 0.5f, 1f);
        Color clamped = _tile.ClampColor(valid);

        Assert.AreEqual(0.5f, clamped.r);
        Assert.AreEqual(0.5f, clamped.g);
        Assert.AreEqual(0.5f, clamped.b);
        Assert.AreEqual(1f, clamped.a);
    }
}