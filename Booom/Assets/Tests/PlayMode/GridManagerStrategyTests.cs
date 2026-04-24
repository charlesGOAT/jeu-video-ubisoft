using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GridManagerStrategyTests
{
    private FakeGridManager _gridManager;
    private GameObject _gridManagerGo;

    [SetUp]
    public void Setup()
    {
        Player.ActivePlayers.Clear();
        Player.PlayerColorDict.Clear();
        
        _gridManagerGo = new GameObject("GridManager");
        _gridManager = _gridManagerGo.AddComponent<FakeGridManager>();
    }

    [TearDown]
    public void Teardown()
    {
        Player.ActivePlayers.Clear();
        Player.PlayerColorDict.Clear();
        Object.Destroy(_gridManagerGo);
    }

    [Test]
    public void WorldToGridCoordinates_ConvertsPositionCorrectly()
    {
        Vector3 worldPos = new Vector3(4f, 0f, 6f);
        Vector2Int gridCoords = GridManagerStrategy.WorldToGridCoordinates(worldPos);

        Assert.AreEqual(new Vector2Int(2, 3), gridCoords);
    }

    [Test]
    public void WorldToGridCoordinates_RoundsToNearest()
    {
        Vector3 worldPos = new Vector3(3.7f, 0f, 5.2f);
        Vector2Int gridCoords = GridManagerStrategy.WorldToGridCoordinates(worldPos);

        Assert.AreEqual(new Vector2Int(2, 3), gridCoords);
    }

    [Test]
    public void GridToWorldPosition_ConvertsToCorrectWorldPosition()
    {
        Vector2Int gridCoords = new Vector2Int(2, 3);
        Vector3 worldPos = GridManagerStrategy.GridToWorldPosition(gridCoords);

        Assert.AreEqual(new Vector3(4f, 0f, 6f), worldPos);
    }

    [Test]
    public void GridToWorldPosition_WithCustomY_AdjustsHeight()
    {
        Vector2Int gridCoords = new Vector2Int(2, 3);
        Vector3 worldPos = GridManagerStrategy.GridToWorldPosition(gridCoords, 5f);

        Assert.AreEqual(new Vector3(4f, 5f, 6f), worldPos);
    }

    [Test]
    public void GetTileAtCoordinates_ReturnsNullForInvalidPosition()
    {
        Tile tile = _gridManager.GetTileAtCoordinates(new Vector2Int(100, 100));
        Assert.IsNull(tile);
    }

    [Test]
    public void IsItemAtPos_ReturnsFalseWhenNoItem()
    {
        Assert.IsFalse(_gridManager.IsItemAtPos(Vector2Int.zero));
    }

    [UnityTest]
    public IEnumerator RoundTrip_WorldToGridToWorld_MaintainsConsistency()
    {
        Vector3 originalWorld = new Vector3(10f, 0f, 8f);
        Vector2Int gridCoords = GridManagerStrategy.WorldToGridCoordinates(originalWorld);
        Vector3 convertedBack = GridManagerStrategy.GridToWorldPosition(gridCoords);

        Assert.AreEqual(originalWorld.x, convertedBack.x, 0.001f);
        Assert.AreEqual(originalWorld.z, convertedBack.z, 0.001f);
        yield return null;
    }
}