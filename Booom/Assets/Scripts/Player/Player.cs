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

    //devrait etre dans bombe?
    [SerializeField]
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

    private StateMachine _stateMachine;
    private IdleState _idleState;
    private HitState _hitState;
    private RunState _runState;
    private JumpState _jumpState;

    //nom de caca
    private float _actualImmuneTimer;

    public static List<Player> ActivePlayers = new List<Player>();
    private bool _isJumping;

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
        CheckStartConditions();
        if(GameManager.Instance.isSpreadingMode) InitializeSpawner();
    }

    private void CheckStartConditions()
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

    private void InitializeSpawner()
    {
        int intPlayerNb = (int)PlayerNb - 1;
        bool isMod2Zero = intPlayerNb % 2 == 0;
        
        var posY = isMod2Zero
            ? GameManager.Instance.GridManager.MapUpperLimit.y
            : GameManager.Instance.GridManager.MapLowerLimit.y;

        int mult = isMod2Zero ? intPlayerNb / 2 : (intPlayerNb + 1) / 2;
        var coord = new Vector2Int(GameManager.Instance.GridManager.MapUpperLimit.x * mult, posY);
        GameManager.Instance.GridManager.GetTileAtCoordinates(coord).OnExplosion(playerNb);
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

    public void OnHit(Vector2Int hitDirection)
    {
        //etant donne que hitDirection est un Vector2Int, y est z dans se cas
        if (IsImmune)
        {
            return;
        }
        _stateMachine.Trigger(GameConstants.PLAYER_HIT_TRIGGER);
        Vector3 forceDirection = new Vector3(hitDirection.x, 1, hitDirection.y);
        _rigidbody.AddForce(forceDirection * knockbackForce, ForceMode.Impulse);
        IsImmune = true;
        _actualImmuneTimer = immuneTimer;
    }

    public void OnJump(Vector2Int jumpDirection) 
    {
        //Formule pour trouver la vitesse initiale quand le sommet du saut est a (Obstacle.ObstacleHeight / 2) + 1 et au demi du trajet
        //position pour gravity 0.5*a*t^2
        float posForGravity = -(Physics.gravity.y / 2) * Mathf.Pow(GameConstants.AIR_STATE_DURATION / 2, 2);

        //position pour apogee du saut
        float jumpHeight = (Obstacle.ObstacleHeight / 2) + 1;

        float velocityY = posForGravity + jumpHeight / (GameConstants.AIR_STATE_DURATION/2);
        float velocityX = (Tile.TileLength * 3) / GameConstants.AIR_STATE_DURATION;
        Vector3 jumpInitialVelocity = new(velocityX * jumpDirection.x, velocityY, jumpDirection.y * velocityX);
        
        _rigidbody.AddForce(jumpInitialVelocity, ForceMode.Impulse);
        _stateMachine.Trigger(GameConstants.PLAYER_JUMP_TRIGGER);
    }

    public void OnPortal(Vector2Int jumpDirection, Vector3 portalPosition) 
    {
        portalPosition.y += (this.transform.localScale.y / 2) + 1;
        this.gameObject.transform.position = portalPosition;

        //Formule pour trouver la vitesse initiale quand le sommet du saut est a (Obstacle.ObstacleHeight / 2) + 1 et au demi du trajet
        //position pour gravity 0.5*a*t^2
        float posForGravity = -(Physics.gravity.y / 2) * Mathf.Pow(GameConstants.AIR_STATE_DURATION / 2, 2);

        //position pour apogee du saut
        float jumpHeight = (Obstacle.ObstacleHeight / 2) + 1;

        float velocityY = posForGravity + jumpHeight / (GameConstants.AIR_STATE_DURATION / 2);
        float velocityX = (Tile.TileLength * 3) / GameConstants.AIR_STATE_DURATION;
        Vector3 jumpInitialVelocity = new(velocityX * jumpDirection.x, velocityY, jumpDirection.y * velocityX);

        _rigidbody.AddForce(jumpInitialVelocity, ForceMode.Impulse);
        _stateMachine.Trigger(GameConstants.PLAYER_JUMP_TRIGGER);
    }

    public void DisableInputActions() => _playerInput.actions.Disable();
    public void EnableInputActions() => _playerInput.actions.Enable();

    private void Update()
    {
        if (CheckIfSpike())
        {
            Vector2Int gridCoordinates = GridManagerStategy.WorldToGridCoordinates(transform.position);
            Spike spike = GameManager.Instance.GridManager.GetTileAtCoordinates<Spike>(gridCoordinates);
            spike.Bruh(this);
        }
        if (CheckIfTrampoline() && !_isJumping) 
        {
            _isJumping = true;
            Vector2Int gridCoordinates = GridManagerStategy.WorldToGridCoordinates(transform.position);
            Trampoline tile = GameManager.Instance.GridManager.GetTileAtCoordinates<Trampoline>(gridCoordinates);
            tile.Bruh(this);
        }
        if (CheckIfPortal() && !_isJumping) 
        {
            _isJumping = true;
            Vector2Int gridCoordinates = GridManagerStategy.WorldToGridCoordinates(transform.position);
            Portal tile = GameManager.Instance.GridManager.GetTileAtCoordinates<Portal>(gridCoordinates);
            tile.Bruh(this);
        }

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
        if (!other.CompareTag("Item") || !other.gameObject.TryGetComponent(out Item item)) return;

        playerItemsManager.AddNewItem(item);
        GameManager.Instance.RemoveItemFromGrid(item.ItemType);
        Destroy(other.gameObject);
    }

    private bool CheckIfOnOwnColor()
    {
        Vector2Int gridCoordinates = GridManagerStategy.WorldToGridCoordinates(transform.position);
        ColorableTile tile = GameManager.Instance.GridManager.GetTileAtCoordinates<ColorableTile>(gridCoordinates);
        if (tile == null)
        {
            return false;
        }

        return tile.CurrentTileOwner == playerNb;
    }

    private bool CheckIfSpike() 
    {
        Vector2Int gridCoordinates = GridManagerStategy.WorldToGridCoordinates(transform.position);
        Spike tile = GameManager.Instance.GridManager.GetTileAtCoordinates<Spike>(gridCoordinates);
        return tile != null;
    }

    private bool CheckIfTrampoline() 
    {
        Vector2Int gridCoordinates = GridManagerStategy.WorldToGridCoordinates(transform.position);
        Trampoline tile = GameManager.Instance.GridManager.GetTileAtCoordinates<Trampoline>(gridCoordinates);
        return tile != null;
    }

    private bool CheckIfPortal() 
    {
        Vector2Int gridCoordinates = GridManagerStategy.WorldToGridCoordinates(transform.position);
        Portal tile = GameManager.Instance.GridManager.GetTileAtCoordinates<Portal>(gridCoordinates);
        return tile != null;
    }

    public Tile GetPlayerTile() => GameManager.Instance.GridManager.GetTileAtCoordinates(GridManagerStategy.WorldToGridCoordinates(transform.position));

    private void InitializeStateMachine()
    {
        _stateMachine = new StateMachine();
        _idleState = new IdleState(_stateMachine, this);
        _hitState = new HitState(_stateMachine, this);
        _runState = new RunState(_stateMachine, this);
        _jumpState = new JumpState(_stateMachine, this);

        _stateMachine.AddTransition<IdleState>(GameConstants.PLAYER_RUN_TRIGGER, _runState);
        _stateMachine.AddTransition<RunState>(GameConstants.PLAYER_IDLE_TRIGGER, _idleState);
        _stateMachine.AddTransition<HitState>(GameConstants.PLAYER_IDLE_TRIGGER, _idleState);
        _stateMachine.AddTransition<JumpState>(GameConstants.PLAYER_IDLE_TRIGGER, _idleState);

        _stateMachine.AddForEachType(GameConstants.PLAYER_HIT_TRIGGER, _hitState);
        _stateMachine.AddForEachType(GameConstants.PLAYER_JUMP_TRIGGER, _jumpState);
        
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
                case 2:
                    playerNb = PlayerEnum.Player3;
                    playerColor = Color.blue;
                    break;
                case 3:
                    playerNb = PlayerEnum.Player4;
                    playerColor = Color.yellow;
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
    Player2 = 2, // add more
    Player3 = 3,
    Player4 = 4
}

