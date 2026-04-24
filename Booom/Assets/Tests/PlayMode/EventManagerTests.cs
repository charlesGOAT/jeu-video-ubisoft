using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EventManagerTests
{
    private EventManager _eventManager;
    private GameObject _eventManagerGo;
    private GameObject _gameManagerGo;
    private GameObject _uiManagerGo;
    private GameObject _soundManagerGo;

    [SetUp]
    public void Setup()
    {
        Player.ActivePlayers.Clear();
        Player.PlayerColorDict.Clear();
        
        _soundManagerGo = new GameObject("SoundManager");
        _soundManagerGo.AddComponent<SoundManager>();
        
        _gameManagerGo = new GameObject("GameManager");
        _gameManagerGo.AddComponent<GameManager>();
        
        _uiManagerGo = new GameObject("GameUIManager");
        _uiManagerGo.AddComponent<GameUIManager>();
        
        _eventManagerGo = new GameObject("EventManager");
        _eventManager = _eventManagerGo.AddComponent<EventManager>();
    }

    [TearDown]
    public void Teardown()
    {
        Player.ActivePlayers.Clear();
        Player.PlayerColorDict.Clear();
        Object.Destroy(_eventManagerGo);
        Object.Destroy(_uiManagerGo);
        Object.Destroy(_gameManagerGo);
        Object.Destroy(_soundManagerGo);
    }

    [Test]
    public void EventManager_InitialBombType_IsNormalBomb()
    {
        Assert.AreEqual(BombEnum.NormalBomb, _eventManager.CurrentBombType);
    }

    [Test]
    public void Start_InitializesBombEventsDictionary()
    {
        _eventManager.Start();
        
        Assert.IsNotNull(_eventManager);
    }

    [Test]
    public void Start_InitializesTextEventsDictionary()
    {
        _eventManager.Start();
        
        Assert.IsNotNull(_eventManager);
    }
}