using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

public class ScoreManagerTests : InputTestFixture
{
    private ScoreManager _scoreManager;
    private GameObject _gameManagerGo;

    [UnitySetUp]
    public override void Setup()
    {
        base.Setup();
        
        Player.ActivePlayers.Clear();
        Player.PlayerColorDict.Clear();
        
        _gameManagerGo = new GameObject("GameManager");
        _gameManagerGo.AddComponent<GameManager>();
        
        _scoreManager = _gameManagerGo.AddComponent<ScoreManager>();
    }

    [UnityTearDown]
    public override void TearDown()
    {
        Player.ActivePlayers.Clear();
        Player.PlayerColorDict.Clear();
        Object.Destroy(_scoreManager);
        Object.Destroy(_gameManagerGo);
        base.TearDown();
    }

    [UnityTest]
    public IEnumerator Awake_InitializesAllPlayerDictionaries()
    {
        _scoreManager.Awake();

        foreach (PlayerEnum player in System.Enum.GetValues(typeof(PlayerEnum)))
        {
            Assert.IsTrue(_scoreManager.AcquiredTilesByPlayer.ContainsKey(player));
            Assert.AreEqual(0, _scoreManager.AcquiredTilesByPlayer[player].Count);
        }

        yield return null;
    }

    [UnityTest]
    public IEnumerator AcquireNewTile_AddsTileToPlayer()
    {
        Vector2Int tilePos = new Vector2Int(1, 1);
        PlayerEnum player = PlayerEnum.Player1;

        _scoreManager.AcquireNewTile(player, tilePos);

        Assert.IsTrue(_scoreManager.AcquiredTilesByPlayer[player].Contains(tilePos));
        Assert.AreEqual(1, _scoreManager.AcquiredTilesByPlayer[player].Count);
        yield return null;
    }

    [UnityTest]
    public IEnumerator AcquireNewTile_IgnoresNonePlayer()
    {
        Vector2Int tilePos = new Vector2Int(1, 1);

        _scoreManager.AcquireNewTile(PlayerEnum.None, tilePos);

        Assert.AreEqual(0, _scoreManager.AcquiredTilesByPlayer[PlayerEnum.None].Count);
        yield return null;
    }

    [UnityTest]
    public IEnumerator LoseTile_RemovesTileFromPlayer()
    {
        Vector2Int tilePos = new Vector2Int(1, 1);
        PlayerEnum player = PlayerEnum.Player1;

        _scoreManager.AcquireNewTile(player, tilePos);
        Assert.AreEqual(1, _scoreManager.AcquiredTilesByPlayer[player].Count);

        _scoreManager.LoseTile(player, tilePos);

        Assert.IsFalse(_scoreManager.AcquiredTilesByPlayer[player].Contains(tilePos));
        Assert.AreEqual(0, _scoreManager.AcquiredTilesByPlayer[player].Count);
        yield return null;
    }

    [UnityTest]
    public IEnumerator LoseTile_IgnoresNonePlayer()
    {
        Vector2Int tilePos = new Vector2Int(1, 1);

        _scoreManager.LoseTile(PlayerEnum.None, tilePos);

        yield return null;
    }

    [UnityTest]
    public IEnumerator FindPlayerWithMostGround_ReturnsPlayerWithMostTiles()
    {
        PlayerEnum player1 = PlayerEnum.Player1;
        PlayerEnum player2 = PlayerEnum.Player2;

        _scoreManager.AcquireNewTile(player1, new Vector2Int(1, 1));
        _scoreManager.AcquireNewTile(player1, new Vector2Int(2, 2));
        _scoreManager.AcquireNewTile(player2, new Vector2Int(3, 3));

        PlayerEnum winner = _scoreManager.FindPlayerWithMostGround();

        Assert.AreEqual(player1, winner);
        yield return null;
    }

    [UnityTest]
    public IEnumerator FindPlayerWithMostGround_ReturnsNoneWhenNoTiles()
    {
        PlayerEnum winner = _scoreManager.FindPlayerWithMostGround();

        Assert.AreEqual(PlayerEnum.None, winner);
        yield return null;
    }

    [UnityTest]
    public IEnumerator OnScoreChanged_EventIsInvoked()
    {
        bool eventFired = false;
        _scoreManager.OnScoreChanged += (player, score) => eventFired = true;

        _scoreManager.AcquireNewTile(PlayerEnum.Player1, new Vector2Int(1, 1));

        Assert.IsTrue(eventFired);
        yield return null;
    }

    [UnityTest]
    public IEnumerator NewElimination_IncrementsPlayerKillCount()
    {
        PlayerEnum player = PlayerEnum.Player1;
        Player playerMock = new GameObject().AddComponent<Player>();
        Player.ActivePlayers[player] = playerMock;
        int initialKills = playerMock.NbKills;

        _scoreManager.NewElimination(player);

        Assert.AreEqual(initialKills + 1, playerMock.NbKills);
        yield return null;
    }

    [UnityTest]
    public IEnumerator NewElimination_IgnoresNonePlayer()
    {
        _scoreManager.NewElimination(PlayerEnum.None);
        yield return null;
    }
}