using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Renderer))]
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

    private Rigidbody _rigidbody;

    public Vector2 MoveInput { get; private set; }
    public bool BombInput { get; private set; } = false;

    private float _nextBombAllowedTime = 0f;
    private GridManagerStategy _gridManager;

    private StateMachine _stateMachine;
    private IdleState _idleState;
    private HitState _hitState;
    private RunState _runState;
    private Renderer _renderer;


    [SerializeField]
    private float immuneTimer = 5;

    //nom de caca
    private float _actualImmuneTimer;

    [SerializeField]
    private int _knockbackForce = 3;

    [SerializeField]
    private int _hitFlickerFrequency = 50;

    public static List<Player> ActivePlayers = new List<Player>();
    

    public bool IsImmune { get; private set; } = false;

    public static readonly Dictionary<PlayerEnum, Color> PlayerColorDict = new Dictionary<PlayerEnum, Color>();  // make it the other way around if we want to test color spreading

    private void Start()
    {
        _gridManager = FindFirstObjectByType<GridManagerStategy>();
        _rigidbody = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();

        if (_gridManager == null)
        {
            throw new Exception("There's no active grid manager");
        }
        
        if(bombPrefab == null)
        {
            Debug.LogError("Bomb prefab shouldn't be null deactivating component");
            enabled = false;
        }

        if (playerNb == PlayerEnum.None)
        {
            throw new Exception("Player cannot be set to PlayerEnum.None");
        }

        if (PlayerColorDict.ContainsKey(playerNb))
        {
            throw new Exception("Two players can't have the same player number.");
        }

        InitializeStateMachine();
        ActivePlayers.Add(this);
        _actualImmuneTimer = immuneTimer;
        PlayerColorDict[playerNb] = playerColor;
        bombPrefab.associatedPlayer = playerNb;
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        MoveInput = ctx.ReadValue<Vector2>();
    }

    public void OnBomb(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            BombInput = true;
        }
    }

    public void TryPlaceBomb()
    {
        if (Time.time < _nextBombAllowedTime)
        {
            return;
        }

        Vector2Int gridCoordinates = GridManagerStategy.WorldToGridCoordinates(transform.position);
        Tile tile = _gridManager.GetTileAtCoordinates(gridCoordinates);

        if (tile == null || tile.isObstacle || Bomb.IsBombAt(gridCoordinates))
        {
            return;
        }

        Vector3 worldPosition = GridManagerStategy.GridToWorldPosition(gridCoordinates, tile.transform.position.y);
        Instantiate(bombPrefab, worldPosition, Quaternion.identity);
        _nextBombAllowedTime = Time.time + bombCooldown;
    }
    
    private void Update()
    {
        UpdateImmune();
        _stateMachine.UpdateStateMachine(Time.deltaTime);
        BombInput = false;
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
        //Étant donné que hitDirection est un Vector2Int, y est z dans se cas
        if (IsImmune)
        {
            return;
        }
        Vector3 forceDirection = new Vector3(hitDirection.x,1,hitDirection.y);
        _rigidbody.AddForce(forceDirection * _knockbackForce, ForceMode.Impulse);
        _stateMachine.Trigger(GameConstants.PLAYER_HIT_TRIGGER);
        IsImmune = true;
        _actualImmuneTimer = immuneTimer;
    }

    public void FlickerPlayerOnHit(float elapsedT) => _renderer.enabled = Mathf.Sin(elapsedT * _hitFlickerFrequency) > 0;

    private void SetRendererVisible() => _renderer.enabled = true;

    public bool IsMoving() => MoveInput.sqrMagnitude > 0.01f;

    public void UpdateMovement()
    {
        Vector2 curMoveInput = MoveInput.normalized;

        float boost = CheckIfOnOwnColor() ? GameConstants.COLOR_BOOST : 1;

        Vector2 move = curMoveInput * (speed * Time.deltaTime * boost);
        transform.position += new Vector3(move.y, 0, -move.x);
    }

    private bool CheckIfOnOwnColor()
    {
        Vector2Int gridCoordinates = GridManagerStategy.WorldToGridCoordinates(transform.position);
        Tile tile = _gridManager.GetTileAtCoordinates(gridCoordinates);
        if (tile == null)
        {
            return false;
        }

        return tile.CurrentTileOwner == playerNb;
    }

    public Tile GetPlayerTile() => _gridManager.GetTileAtCoordinates(GridManagerStategy.WorldToGridCoordinates(transform.position));


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
}

public enum PlayerEnum
{
    None = 0,
    Player1 = 1,
    Player2 = 2 // add more
}