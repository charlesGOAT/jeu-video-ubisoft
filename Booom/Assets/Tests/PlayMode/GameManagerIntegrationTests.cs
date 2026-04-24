using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

public class GameManagerIntegrationTests : InputTestFixture
{
    private GameObject _gameManagerGo;
    private GameObject _gridGo;
    private GameObject _bombGo;
    private GameObject _playerGo;
    private Player _player;
    private Keyboard _keyboard;

    [UnitySetUp]
    public override void Setup()
    {
        base.Setup();
        
        Player.ActivePlayers.Clear();
        Player.PlayerColorDict.Clear();
        
        _keyboard = InputSystem.AddDevice<Keyboard>();
        
        _gridGo = new GameObject("GridManager");
        _gridGo.AddComponent<FakeGridManager>();
        
        _bombGo = new GameObject("BombManager");
        _bombGo.AddComponent<FakeBombManager>();
        
        _gameManagerGo = new GameObject("GameManager");
        _gameManagerGo.AddComponent<GameManager>();
        
        _playerGo = new GameObject("Player");
        _playerGo.AddComponent<CharacterController>();
        _playerGo.AddComponent<PlayerItemsManager>();
        _playerGo.AddComponent<MeshRenderer>();
        
        var input = _playerGo.AddComponent<PlayerInput>();
        input.actions = CreateTestInputActions();
        input.defaultActionMap = "Player";
        input.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
        
        _player = _playerGo.AddComponent<Player>();
    }

    [UnityTearDown]
    public override void TearDown()
    {
        Object.Destroy(_playerGo);
        Object.Destroy(_gameManagerGo);
        Object.Destroy(_gridGo);
        Object.Destroy(_bombGo);
        base.TearDown();
    }

    [UnityTest]
    public IEnumerator GameManager_HasAllManagersInitialized()
    {
        var gm = GameManager.Instance;
        
        Assert.IsNotNull(gm.GridManager);
        Assert.IsNotNull(gm.BombManager);
        Assert.IsNotNull(gm.ItemsManager);
        Assert.IsNotNull(gm.ScoreManager);
        Assert.IsNotNull(gm.GameUIManager);
        Assert.IsNotNull(gm.EventManager);
        
        yield return null;
    }

    [UnityTest]
    public IEnumerator GameManager_Singleton_ReturnsSameInstance()
    {
        var instance1 = GameManager.Instance;
        var instance2 = GameManager.Instance;
        
        Assert.AreSame(instance1, instance2);
        yield return null;
    }

    [UnityTest]
    public IEnumerator GameManager_CollisionLayers_HasFourLayers()
    {
        var gm = GameManager.Instance;
        
        Assert.AreEqual(4, gm.CollisionLayers.Length);
        yield return null;
    }

    [UnityTest]
    public IEnumerator GameManager_IsSpreadingMode_DefaultIsTrue()
    {
        var gm = GameManager.Instance;
        
        Assert.IsTrue(gm.IsSpreadingMode);
        yield return null;
    }

    [UnityTest]
    public IEnumerator GameManager_GameDuration_DefaultIs60()
    {
        var gm = GameManager.Instance;
        
        Assert.AreEqual(60f, gm.GameDuration);
        yield return null;
    }

    [UnityTest]
    public IEnumerator GameManager_RemoveItemFromGrid_CallsManagers()
    {
        var gm = GameManager.Instance;
        GameObject itemGo = new GameObject("Item");
        Item item = itemGo.AddComponent<Item>();
        
        Assert.DoesNotThrow(() => gm.RemoveItemFromGrid(item));
        
        Object.Destroy(itemGo);
        yield return null;
    }

    private InputActionAsset CreateTestInputActions()
    {
        var asset = ScriptableObject.CreateInstance<InputActionAsset>();

        var map = new InputActionMap("Player");

        var move = map.AddAction("Move");
        move.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        var bomb = map.AddAction("PlaceBomb", binding: "<Keyboard>/space");

        asset.AddActionMap(map);
        map.Enable();

        return asset;
    }
}