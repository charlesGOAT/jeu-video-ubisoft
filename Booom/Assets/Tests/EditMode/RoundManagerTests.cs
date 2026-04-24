using NUnit.Framework;
using UnityEngine;

public class RoundManagerTests
{
    [Test]
    public void MapsToPlay_HasCorrectMaps()
    {
        Assert.Greater(RoundManager.MapsToPlay.Count, 0);
        Assert.AreEqual(5, RoundManager.MapsToPlay.Count);
    }

    [Test]
    public void FindNextMap_ReturnsValidMapIndex()
    {
        int mapIndex = RoundManager.FindNextMap();
        
        Assert.IsTrue(RoundManager.MapsToPlay.Contains(mapIndex));
    }

    [Test]
    public void ShouldEndGame_AfterTwoWins_ReturnsTrue()
    {
        PlayerEnum player = PlayerEnum.Player1;
        
        bool firstWin = RoundManager.ShouldEndGame(player);
        bool secondWin = RoundManager.ShouldEndGame(player);
        
        Assert.IsFalse(firstWin);
        Assert.IsTrue(secondWin);
    }

    [Test]
    public void ShouldEndGame_AddsPlayerToGameWonPlayer()
    {
        PlayerEnum player = PlayerEnum.Player2;
        int initialCount = RoundManager.GameWonPlayer.Count;
        
        RoundManager.ShouldEndGame(player);
        
        Assert.Greater(RoundManager.GameWonPlayer.Count, initialCount);
    }

    [Test]
    public void CleanGame_ResetsAllState()
    {
        RoundManager.FindNextMap();
        RoundManager.ShouldEndGame(PlayerEnum.Player1);
        
        RoundManager.CleanGame();
        
        Assert.AreEqual(0, RoundManager.GameWonPlayer.Count);
        Assert.AreEqual(0, RoundManager.LastMapIndex);
    }
}