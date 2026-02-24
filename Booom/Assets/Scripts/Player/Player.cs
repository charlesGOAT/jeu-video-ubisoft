using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public delegate void MoveCalledEventHandler(Player player);

[RequireComponent(typeof(PlayerItemsManager))]
[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;

    [SerializeField]
    private Bomb bombPrefab;

    [SerializeField]
    private Color playerColor = Color.red;

    [SerializeField]
    private PlayerEnum playerNb = PlayerEnum.None;

    [SerializeField]
    private PlayerItemsManager playerItemsManager;

    [SerializeField]
    private int knockbackForce = 10;

    [SerializeField]
    private int hitFlickerFrequency = 50;

    [SerializeField]
    private float immuneTimer = 5;

    [SerializeField]
    private float tileDetectionTolerance = 0.35f;

    [SerializeField]
    private GameObject arrow;

    private PlayerInput _playerInput;
    private Renderer _renderer;

    private Vector2 _moveInput;
    private Vector3 _lastInput;
    private BombEnum _currentBombType = BombEnum.NormalBomb;

    private int _bombTypeCount;

    public PlayerEnum PlayerNb => playerNb;

    private CharacterController _characterController;
    private Vector3 _knockbackVelocity;
    private Vector3 _jumpVelocity;
    private float _knockbackDamping = 8f;
    private float _verticalVelocity;

    private StateMachine _stateMachine;
    private IdleState _idleState;
    private HitState _hitState;
    private RunState _runState;
    private JumpState _jumpState;

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

        InitializeArrow();
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
        GameManager.Instance.GridManager.GetTileAtCoordinates(coord).ChangeTileColor(playerNb);
    }

    private void InitializeArrow()
    {
        foreach (var children in arrow.GetComponentsInChildren<Renderer>())
        {
            children.material.color = playerColor;
        }
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
        if (_moveInput != Vector2.zero) 
        {
            _lastInput = GetBombPlacementDirection(_moveInput);
        }
    
        RotateArrow();
    }
    
    private void RotateArrow()
    {
        Vector3 targetDirection = _lastInput * (float)(Tile.TileLength / 2.0);
        if (targetDirection != Vector3.zero) 
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);

            arrow.transform.rotation = targetRotation;
            arrow.transform.position = transform.position + targetDirection;
        }
    }

    public void OnBomb(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Vector3 bombDirection = _moveInput.sqrMagnitude > 0.0001f ? GetBombPlacementDirection(_moveInput) : _lastInput;
            GameManager.Instance.BombManager.CreateBomb(transform.position + (bombDirection * Tile.TileLength), playerNb, _currentBombType);
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
        if (GetPlayerTile() != null) 
        {
            GetPlayerTile().StepOnTile(this);
        }
        
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
        Vector3 forceDirection = new Vector3(hitDirection.x,1,hitDirection.y);
        ApplyKnockback(forceDirection, knockbackForce);
        _stateMachine.Trigger(GameConstants.PLAYER_HIT_TRIGGER);
        IsImmune = true;
        _actualImmuneTimer = immuneTimer;
    }

    public void OnJump(Vector2Int jumpDirection) 
    {
        if (_stateMachine.CurrentState is not JumpState)
        {
            _jumpVelocity = CalculateJumpForce(jumpDirection);
            _stateMachine.Trigger(GameConstants.PLAYER_JUMP_TRIGGER);
        }
    }

    public void OnPortal(Vector2Int playerDirection, Vector3 otherPortalPosition)
    {
        if (_stateMachine.CurrentState is not JumpState) 
        {
            _characterController.enabled = false;
            this.gameObject.transform.position = otherPortalPosition;
            _jumpVelocity = CalculatePortalForce(playerDirection);
            _stateMachine.Trigger(GameConstants.PLAYER_JUMP_TRIGGER);
            _characterController.enabled = true;
        }
    }

    private Vector3 CalculateJumpForce(Vector2Int jumpDirection)
    {
        //Formule pour trouver la vitesse initiale quand le sommet du saut est a (Obstacle.ObstacleHeight / 2) + 1 et au demi du trajet
        //position pour gravity 0.5*a*t^2
        float posForGravity = -(Physics.gravity.y / 2) * Mathf.Pow(GameConstants.AIR_STATE_DURATION / 2, 2);

        //position pour apogee du saut
        float jumpHeight = (Obstacle.ObstacleHeight) + GameConstants.JUMP_HEIGHT_OFFSET;

        float velocityY = posForGravity + jumpHeight / (GameConstants.AIR_STATE_DURATION / 2);

        float velocityX = (Tile.TileLength * GameConstants.JUMP_NUMBER_OF_TILES) /(GameConstants.AIR_STATE_DURATION);
        Vector3 jumpInitialVelocity = new(velocityX * jumpDirection.x, velocityY, jumpDirection.y * velocityX);


        return jumpInitialVelocity;
    }

    private Vector3 CalculatePortalForce(Vector2Int jumpDirection)
    {
        //Formule pour trouver la vitesse initiale quand le sommet du saut est a (Obstacle.ObstacleHeight / 2) + 1 et au demi du trajet
        //position pour gravity 0.5*a*t^2
        float posForGravity = -(Physics.gravity.y / 2) * Mathf.Pow(GameConstants.PORTAL_AIR_DURATION / 2, 2);

        float velocityY = (posForGravity + GameConstants.PORTAL_JUMP_HEIGHT) / (GameConstants.PORTAL_AIR_DURATION / 2);

        float velocityX = Tile.TileLength / GameConstants.PORTAL_AIR_DURATION;
        Vector3 jumpInitialVelocity = new(velocityX * jumpDirection.x, velocityY, jumpDirection.y * velocityX);


        return jumpInitialVelocity;
    }


    public void UpdateJump() 
    {
        float moveY = ApplyGravity(ref _jumpVelocity.y);
        Vector3 jumpMove = new Vector3(_jumpVelocity.x * Time.deltaTime, moveY, _jumpVelocity.z * Time.deltaTime);
        _characterController.Move(jumpMove);
    }

    public void ResetJumpVelocity() => _jumpVelocity = Vector3.zero;


    public void FlickerPlayerOnHit(float elapsedT) => _renderer.enabled = Mathf.Sin(elapsedT * hitFlickerFrequency) > 0;

    private void SetRendererVisible() => _renderer.enabled = true;

    public bool IsMoving() => _moveInput.sqrMagnitude > 0.01f;

    public void UpdateMovement()
    {
        Vector2 curMoveInput = _moveInput.normalized;

        float boost = CheckIfOnOwnColor() ? GameConstants.COLOR_BOOST : (CheckIfOnEnemyTerritory() ? GameConstants.COLOR_DEBUFF: 1);

        Vector3 move = new Vector3(curMoveInput.y, 0, -curMoveInput.x) * (speed * boost);
        float tempMove = ApplyGravity(ref _verticalVelocity);

        _characterController.Move(move * Time.deltaTime);
        _characterController.Move(Vector3.down * Math.Abs(tempMove));
        OnMoveFunctionCalled?.Invoke(this);
    }

    //Peut etre faire une meilleure fonction
    private float ApplyGravity(ref float currentVerticalVelocity)
    {
        float tempMove;
        if (GetIsGrounded() && currentVerticalVelocity < 0f)
        {
            tempMove = 0f;
            currentVerticalVelocity = 0f;
        }
        else
        {
            //calcul de la gravité par rapport a la position = 0.5*at^2 + vt
            tempMove = (0.5f * Physics.gravity.y * Time.deltaTime * Time.deltaTime) + (currentVerticalVelocity * Time.deltaTime);
            currentVerticalVelocity += Physics.gravity.y * Time.deltaTime;
        }

        return tempMove;
    }

    public bool GetIsGrounded()
    {
        return _characterController.isGrounded;
    }

    public void UpdateKnockback()
    {
        Vector3 move = _knockbackVelocity;
        _knockbackVelocity = Vector3.Lerp(
            _knockbackVelocity,
            Vector3.zero,
            _knockbackDamping * Time.deltaTime
        );

        move.y = _verticalVelocity;
        
        _characterController.Move(move * Time.deltaTime);
    }
    
    public void ApplyKnockback(Vector3 forceDirection, float force)
    {
        _knockbackVelocity = forceDirection.normalized * force;
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
        Vector2Int gridCoordinates = GridManagerStrategy.WorldToGridCoordinates(transform.position);
        Tile tile = GameManager.Instance.GridManager.GetTileAtCoordinates(gridCoordinates);
        if (tile == null)
        {
            return false;
        }

        return tile.CurrentTileOwner == playerNb;
    }

    private bool CheckIfOnEnemyTerritory() 
    {
        Vector2Int gridCoordinates = GridManagerStrategy.WorldToGridCoordinates(transform.position);
        Tile tile = GameManager.Instance.GridManager.GetTileAtCoordinates(gridCoordinates);
        if (tile == null)
        {
            return false;
        }

        return tile.CurrentTileOwner != playerNb && tile.CurrentTileOwner != PlayerEnum.None;
    }

    public Tile GetPlayerTile()
    {
        Vector2Int gridCoordinates = GridManagerStrategy.WorldToGridCoordinates(transform.position);
        Tile tile = GameManager.Instance.GridManager.GetTileAtCoordinates(gridCoordinates);
        if (tile == null)
        {
            return null;
        }

        float playerFeetY = _characterController.bounds.min.y;
        float tileSurfaceY = tile.transform.position.y;

        return Mathf.Abs(playerFeetY - tileSurfaceY) <= tileDetectionTolerance ? tile : null;
    }

    private Vector3 GetBombPlacementDirection(Vector2 input)
    {
        //a cause de la camera les inputs sont weird
        Vector3 worldDirection = new(input.y, 0f, -input.x);
        float absX = Mathf.Abs(worldDirection.x);
        float absZ = Mathf.Abs(worldDirection.z);

        if (absX < 0.001f && absZ < 0.001f)
        {
            return _lastInput;
        }

        if (absX < 0.001f)
        {
            return new Vector3(0f, 0f, Mathf.Sign(worldDirection.z));
        }

        if (absZ < 0.001f)
        {
            return new Vector3(Mathf.Sign(worldDirection.x), 0f, 0f);
        }

        Vector3 xCandidate = new(Mathf.Sign(worldDirection.x), 0f, 0f);
        Vector3 zCandidate = new(0f, 0f, Mathf.Sign(worldDirection.z));

        var position = transform.position;
        Vector3 intendedTarget = position + worldDirection.normalized * Tile.TileLength;
        Vector3 xTarget = position + xCandidate * Tile.TileLength;
        Vector3 zTarget = position + zCandidate * Tile.TileLength;

        float xDistance = (intendedTarget - xTarget).sqrMagnitude;
        float zDistance = (intendedTarget - zTarget).sqrMagnitude;

        if (Mathf.Abs(xDistance - zDistance) <= 0.0001f)
        {
            return absX >= absZ ? xCandidate : zCandidate;
        }

        return xDistance < zDistance ? xCandidate : zCandidate;
    }

    private void InitializeStateMachine()
    {
        _stateMachine = new StateMachine();
        _idleState = new IdleState(_stateMachine, this);
        _hitState = new HitState(_stateMachine, this);
        _runState = new RunState(_stateMachine, this);
        _jumpState = new JumpState(_stateMachine, this);

        _stateMachine.AddTransition<IdleState>(GameConstants.PLAYER_RUN_TRIGGER, _runState);
        _stateMachine.AddTransition<RunState>(GameConstants.PLAYER_IDLE_TRIGGER, _idleState);
        _stateMachine.AddTransition<JumpState>(GameConstants.PLAYER_IDLE_TRIGGER, _idleState);
        _stateMachine.AddTransition<JumpState>(GameConstants.PLAYER_RUN_TRIGGER, _runState);
        _stateMachine.AddForEachType(GameConstants.PLAYER_JUMP_TRIGGER, _jumpState);
        _stateMachine.AddTransition<HitState>(GameConstants.PLAYER_IDLE_TRIGGER, _idleState);
        _stateMachine.AddForEachType(GameConstants.PLAYER_HIT_TRIGGER, _hitState);
        _stateMachine.SetInitialState(_idleState);
    }

    private void GetComponents()
    {
        _characterController = GetComponent<CharacterController>();
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
        
        gameObject.GetComponent<Renderer>().material.color = playerColor;
    }
}

public enum PlayerEnum
{
    None = 0,
    Player1 = 1,
    Player2 = 2,
    Player3 = 3,
    Player4 = 4
}


