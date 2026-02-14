using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public delegate void MoveCalledEventHandler(Player player);

[RequireComponent(typeof(PlayerItemsManager))]
[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;

    [SerializeField]
    private Bomb bombPrefab;

    [SerializeField]
    private float bombCooldown = 3f;

    [SerializeField]
    private Color playerColor = Color.red;

    [SerializeField]
    private PlayerEnum playerNb = PlayerEnum.None;

    [SerializeField]
    private PlayerItemsManager playerItemsManager;

    private int knockbackForce = 3;

    [SerializeField]
    private int hitFlickerFrequency = 50;

    [SerializeField]
    private float immuneTimer = 5;

    private Rigidbody _rigidbody;
    private PlayerInput _playerInput;
    private Renderer _renderer;

    private Vector2 _moveInput;
    private BombEnum _currentBombType = BombEnum.NormalBomb;

    private int _bombTypeCount;

    public PlayerEnum PlayerNb => playerNb;

    private GridManagerStategy _gridManager;
    private BombManager _bombManager;

    private StateMachine _stateMachine;
    private IdleState _idleState;
    private HitState _hitState;
    private RunState _runState;

    //nom de caca
    private float _actualImmuneTimer;

    public static List<Player> ActivePlayers = new List<Player>();

    public bool IsImmune { get; private set; } = false;

    public static readonly Dictionary<PlayerEnum, Color> PlayerColorDict = new Dictionary<PlayerEnum, Color>();  // make it the other way around if we want to test color spreading
    
    public event MoveCalledEventHandler OnMoveFunctionCalled;

    private void Awake()
    {
        if (playerItemsManager == null)
            playerItemsManager = gameObject.GetComponent<PlayerItemsManager>();

        playerItemsManager.Player = this;

        _bombTypeCount = Enum.GetValues(typeof(BombEnum)).Length - 1; // -1 to avoid None
        ConfigurePlayers();
        InitializeStateMachine();
        GetComponents();

        ActivePlayers.Add(this);
    }
    
    private void Start()
    {
        if (playerNb == PlayerEnum.None)
        {
            throw new Exception("Player cannot be set to PlayerEnum.None");
        }

        if (!PlayerColorDict.TryAdd(playerNb, playerColor))
        {
            throw new Exception("Player already exists");
        }
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnBomb(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            GameManager.Instance.BombManager.CreateBomb(transform.position, playerNb, _currentBombType);
        }
    }

    public void OnChangeBomb(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            int nextBomb = ((int)_currentBombType % _bombTypeCount) + 1; // +1 to bring back above 0
            _currentBombType = (BombEnum)nextBomb;
        }
    }

    public void DisableInputActions() => _playerInput.actions.Disable();
    public void EnableInputActions() => _playerInput.actions.Enable();

    private void Update()
    {
        UpdateImmune();
        _stateMachine.UpdateStateMachine(Time.deltaTime);
    }

    private void UpdateImmune()
    {
        if (IsImmune)
        {
            if (_actualImmuneTimer <= 0)
            {
                IsImmune = false;
                SetRendererVisible();
            }
            else
            {
                _actualImmuneTimer -= Time.deltaTime;
                FlickerPlayerOnHit(_actualImmuneTimer);
            }
        }
    }

    public void OnHit(Vector2Int hitDirection)
    {
        //etant donne que hitDirection est un Vector2Int, y est z dans se cas
        if (IsImmune)
        {
            return;
        }
        Vector3 forceDirection = new Vector3(hitDirection.x, 1, hitDirection.y);
        _rigidbody.AddForce(forceDirection * knockbackForce, ForceMode.Impulse);
        _stateMachine.Trigger(GameConstants.PLAYER_HIT_TRIGGER);
        IsImmune = true;
        _actualImmuneTimer = immuneTimer;
    }

    public void FlickerPlayerOnHit(float elapsedT) => _renderer.enabled = Mathf.Sin(elapsedT * hitFlickerFrequency) > 0;

    private void SetRendererVisible() => _renderer.enabled = true;

    public bool IsMoving() => _moveInput.sqrMagnitude > 0.01f;

    public void UpdateMovement()
    {
        Vector2 curMoveInput = _moveInput.normalized;

        float boost = CheckIfOnOwnColor() ? GameConstants.COLOR_BOOST : 1;

        Vector2 move = curMoveInput * (speed * Time.deltaTime * boost);
        transform.position += new Vector3(move.y, 0, -move.x);

        OnMoveFunctionCalled?.Invoke(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.tag.Equals("Item") || !other.gameObject.TryGetComponent(out Item item)) return;

        playerItemsManager.AddNewItem(item);
        GameManager.Instance.RemoveItemFromGrid(item.ItemType);
        Destroy(other.gameObject);
    }

    private bool CheckIfOnOwnColor()
    {
        Vector2Int gridCoordinates = GridManagerStategy.WorldToGridCoordinates(transform.position);
        Tile tile = GameManager.Instance.GridManager.GetTileAtCoordinates(gridCoordinates);
        if (tile == null)
        {
            return false;
        }

        return tile.CurrentTileOwner == playerNb;
    }

    public Tile GetPlayerTile() => GameManager.Instance.GridManager.GetTileAtCoordinates(GridManagerStategy.WorldToGridCoordinates(transform.position));

    private void InitializeStateMachine()
    {
        _stateMachine = new StateMachine();
        _idleState = new IdleState(_stateMachine, this);
        _hitState = new HitState(_stateMachine, this);
        _runState = new RunState(_stateMachine, this);

        _stateMachine.AddTransition<IdleState>(GameConstants.PLAYER_RUN_TRIGGER, _runState);
        _stateMachine.AddTransition<RunState>(GameConstants.PLAYER_IDLE_TRIGGER, _idleState);
        _stateMachine.AddTransition<HitState>(GameConstants.PLAYER_IDLE_TRIGGER, _idleState);
        _stateMachine.AddForEachType(GameConstants.PLAYER_HIT_TRIGGER, _hitState);
        _stateMachine.SetInitialState(_idleState);
    }

    private void GetComponents()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();
        _playerInput = GetComponent<PlayerInput>();
    }

    private void ConfigurePlayers()
    {
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            switch (playerInput.playerIndex)
            {
                case 0:
                    playerNb = PlayerEnum.Player1;
                    playerColor = Color.red;
                    break;
                case 1:
                    playerNb = PlayerEnum.Player2;
                    playerColor = Color.green;
                    break;
                default:
                    playerNb = PlayerEnum.None;
                    break;
            }
        }
        else
        {
            throw new Exception("There's no active player input");
        }
    }
}


public enum PlayerEnum
{
    None = 0,
    Player1 = 1,
    Player2 = 2 // add more
}
