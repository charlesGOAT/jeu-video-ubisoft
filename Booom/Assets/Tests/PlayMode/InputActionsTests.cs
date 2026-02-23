using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

public class InputActionTests : InputTestFixture
{
    private GameObject _playerGo;
    private Player _player;
    private Keyboard _keyboard;

    private GameObject _gameManagerGo;
    private GameObject _bombGo;
    private GameObject _gridGo;

    [UnitySetUp]
    public override void Setup()
    {
        base.Setup();
        
        Player.ActivePlayers.Clear();
        Player.PlayerColorDict.Clear();
        
        // ---- Input Device ----
        _keyboard = InputSystem.AddDevice<Keyboard>();

        // ---- Fake Managers ----
        _gridGo = new GameObject("GridManager");
        _gridGo.AddComponent<FakeGridManager>();

        _bombGo = new GameObject("BombManager");
        _bombGo.AddComponent<FakeBombManager>();

        _gameManagerGo = new GameObject("GameManager");
        _gameManagerGo.AddComponent<GameManager>();

        // ---- Player ----
        _playerGo = new GameObject("Player");

        _playerGo.AddComponent<CharacterController>();
        _playerGo.AddComponent<PlayerItemsManager>();
        _playerGo.AddComponent<MeshRenderer>();

        var input = _playerGo.AddComponent<PlayerInput>();
        input.actions = CreateTestInputActions();
        input.defaultActionMap = "Player";
        input.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
        
        _player = _playerGo.AddComponent<Player>();
        
        input.actions["Move"].performed += _player.OnMove;
        input.actions["Move"].canceled += _player.OnMove;
        input.actions["PlaceBomb"].performed += _player.OnBomb;
    }

    [UnityTearDown]
    public override void TearDown()
    {
        Object.Destroy(_playerGo);
        Object.Destroy(_gameManagerGo);
        Object.Destroy(_gridGo);
        Object.Destroy(_bombGo);
    }

    // --------------------------------------------------------
    // INPUT TESTS
    // --------------------------------------------------------

    [UnityTest]
    public IEnumerator MoveInput_UpdatesMoveState()
    {
        Press(_keyboard.wKey);
        yield return null;

        Assert.IsTrue(_player.IsMoving());

        Release(_keyboard.wKey);
        yield return null;

        Assert.IsFalse(_player.IsMoving());
    }

    [UnityTest]
    public IEnumerator BombInput_CallsBombManager()
    {
        var fake = Object.FindFirstObjectByType<FakeBombManager>();

        Press(_keyboard.spaceKey);
        yield return null;

        Assert.IsTrue(fake.bombCreated);
    }

    // --------------------------------------------------------
    // HIT / IMMUNITY TEST
    // --------------------------------------------------------

    [UnityTest]
    public IEnumerator OnHit_MakesPlayerImmune()
    {
        _player.OnHit(Vector2Int.right);
        yield return null;

        Assert.IsTrue(_player.IsImmune);
    }

    [UnityTest]
    public IEnumerator Player_Becomes_Vulnerable_After_ImmuneTimer()
    {
        _player.OnHit(Vector2Int.right);

        yield return new WaitForSeconds(6f); // immuneTimer = 5

        Assert.IsFalse(_player.IsImmune);
    }

    // --------------------------------------------------------
    // STATE MACHINE TEST
    // --------------------------------------------------------

    [UnityTest]
    public IEnumerator HitState_Disables_Input()
    {
        _player.OnHit(Vector2Int.right);
        yield return null;

        Assert.IsFalse(_player.GetComponent<PlayerInput>().actions.enabled);
    }

    [UnityTest]
    public IEnumerator HitState_ReEnables_Input_After_Duration()
    {
        _player.OnHit(Vector2Int.right);

        yield return new WaitForSeconds(GameConstants.HIT_STATE_DURATION + 0.2f);

        Assert.IsTrue(_player.GetComponent<PlayerInput>().actions.enabled);
    }

    // --------------------------------------------------------
    // KNOCKBACK TEST
    // --------------------------------------------------------

    [UnityTest]
    public IEnumerator Knockback_Moves_Player()
    {
        Vector3 initialPos = _player.transform.position;

        _player.OnHit(Vector2Int.right);

        yield return new WaitForSeconds(0.2f);

        Assert.AreNotEqual(initialPos, _player.transform.position);
    }

    // --------------------------------------------------------
    // INPUT DISABLE TEST
    // --------------------------------------------------------

    [UnityTest]
    public IEnumerator DisableInput_PreventsMovement()
    {
        _player.DisableInputActions();

        Press(_keyboard.wKey);
        yield return null;

        Assert.IsFalse(_player.IsMoving());
    }

    // --------------------------------------------------------
    // Helper: Create Input Actions
    // --------------------------------------------------------

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
