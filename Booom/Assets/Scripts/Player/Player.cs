using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public delegate void MoveCalledEventHandler();
public delegate void PlaceBomb();
public delegate void PlaceBombSuccessFul();
public delegate void PlaceBombSuccessFulChained(ItemType itemType);

[RequireComponent(typeof(PlayerItemsManager))]
[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    [SerializeField]
    private float speed = 8f;

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
    private TextMeshProUGUI killStreakText;

    [SerializeField]
    private Image itemTextPopUpBackground;
    
    [SerializeField]
    private TMP_Text itemTextPopUpText;

    [SerializeField]
    private RawImage itemIconPrefab;
    
    [SerializeField]
    private Transform itemIconsContainer;
    
    private Dictionary<ItemType, RawImage> _activeIcons = new();
    private float _popUpDuration = GameConstants.POPUP_DURATION;
    private Coroutine _popUpCoroutine;

    private BombFusingStrategy[] _bombFusingStrategies = new []
        {
            new BombFusingStrategy(),
            new TargetBombFusingStrategy()
            
            // todo : add more here
        };

    [SerializeField]
    private List<Material> playerMaterials;
    private Dictionary<int, float> _speedBoostPerKill = GameConstants.SpeedBoostPerKill;
    private Dictionary<int, int> _rangeBoostPerKill = GameConstants.RangeBoostPerKill;
    private float _airStateDuration = GameConstants.AIR_STATE_DURATION;

    private PlayerInput _playerInput;
    private Renderer[] _renderers;
    private bool[] _rendererDefaultStates;
    private List<Material[]> _initialMats = new();

    private Vector2 _moveInput;
    private Vector3 _lastInput;
    
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

    private Tile _currentTile;
    
    public BombFusingType BombFusingType { get; set; }
    public BombItems NextBombBombItems = 0;

    //nom de caca
    private float _actualImmuneTimer;

    public static List<Player> ActivePlayers = new List<Player>();

    private int _elimsRangeBoost = 0;
    public int ElimsRangeBoost => _elimsRangeBoost;
    private float _elimsSpeedBoost = 1;

    private int _nbKills = 0;
    public int NbKills { 
        get => _nbKills;
        set
        {
            _nbKills = value;
            OnNbKillsChanged(); 
        } 
    }

    public bool IsImmune { get; private set; } = false;
    public Animator Animator { get; private set; }

    public static readonly Dictionary<PlayerEnum, Color> PlayerColorDict = new Dictionary<PlayerEnum, Color>();
    
    public event MoveCalledEventHandler OnMoveFunctionCalled;
    public event PlaceBomb OnPlaceBomb;
    public event PlaceBombSuccessFul OnPlaceBombSuccessful;
    public event PlaceBombSuccessFulChained OnPlaceBombSuccessfulChained;

    public const float JUMP_HEIGHT_OFFSET = 2.5f;
    public const float JUMP_NUMBER_OF_TILES = 2.7f;
    public const float PLAYER_GRAVITY = -36.0f;


    private void Awake()
    {
        if (playerItemsManager == null)
            playerItemsManager = gameObject.GetComponent<PlayerItemsManager>();

        playerItemsManager.Player = this;
        Animator = GetComponentInChildren<Animator>();
        InitializeStateMachine();
        GetComponents();
        ConfigurePlayers();
        ActivePlayers.Add(this);
    }

    private void Start()
    {
#if !UNITY_EDITOR
        GetConfigValues();
#endif
        CheckStartConditions();
        InitializeSpawner();
    }

    private void GetConfigValues()
    {
        var runtimeConfig = GameManager.Instance.RuntimeConfig;
        speed = runtimeConfig.MovementSpeed;
        _rangeBoostPerKill = runtimeConfig.RangeBoostPerKill;
        _speedBoostPerKill = runtimeConfig.SpeedBoostPerKill;
        _popUpDuration = runtimeConfig.PopUpDuration;
        _airStateDuration = runtimeConfig.AirStateDuration;
    }

    private void CheckStartConditions()
    {
        if (playerNb == PlayerEnum.None)
        {
            throw new Exception("Player cannot be set to PlayerEnum.None");
        }
    }

    private void OnNbKillsChanged()
    {
        bool shouldDisplay = false;
        if (GameConstants.SpeedBoostPerKill.TryGetValue(NbKills, out float newSpeedBoost) && GameManager.Instance.IsBonusSpeed)
        {
            _elimsSpeedBoost = newSpeedBoost;
            SoundManager.Instance.OnNewKillStreak();
            shouldDisplay = true;
        }
        if (GameConstants.RangeBoostPerKill.TryGetValue(NbKills, out int newRangeBoost) && !GameManager.Instance.IsBonusSpeed)
        {
            _elimsRangeBoost = newRangeBoost;
            SoundManager.Instance.OnNewKillStreak();
            shouldDisplay = true;
        }
        
        if(shouldDisplay) StartCoroutine(DisplayKillStreak());
        
        // todo : generate little animation or particle effect indicating kill streak
    }

    private IEnumerator DisplayKillStreak()
    {
        killStreakText.text = "NEW KILL BONUS";
        yield return new WaitForSeconds(3f);
        killStreakText.text = "";
    }

    private void InitializeSpawner()
    {
        if (GameManager.Instance.GridManager.playerSpawnPoints.Length < (int)playerNb)
        {
            InitializeSpawnerWithDynamicSpawnPos();
        }
        else
        {
            InitializeSpawnerWithFixedSpawnPos();
        }
    }

    private void InitializeSpawnerWithDynamicSpawnPos()
    {
        int intPlayerNb = (int)PlayerNb - 1;
        bool isMod2Zero = intPlayerNb % 2 == 0;
        
        int posY = isMod2Zero
            ? GameManager.Instance.GridManager.MapUpperLimit.y
            : GameManager.Instance.GridManager.MapLowerLimit.y;

        int mult = isMod2Zero ? intPlayerNb / 2 : (intPlayerNb + 1) / 2;
        Vector2Int spawnPointGrid = new Vector2Int(GameManager.Instance.GridManager.MapUpperLimit.x * mult, posY);

        if (GameManager.Instance.IsSpreadingMode)
        {
            Tile tile = GameManager.Instance.GridManager.GetTileAtCoordinates(spawnPointGrid);
            tile.ChangeTileColor(playerNb);
            tile.IsSpawn = true;
        }
        
        MovePlayerOnSpawnPoint(spawnPointGrid);
    }

    private void InitializeSpawnerWithFixedSpawnPos()
    {
        Vector2Int spawnPointGrid = GameManager.Instance.GridManager.playerSpawnPoints[(int)playerNb - 1];
        Tile tile = GameManager.Instance.GridManager.GetTileAtCoordinates(spawnPointGrid);
        
        if (tile == null)
            throw new Exception($"There's no tile at player spawn point position {spawnPointGrid}");
        if (tile.IsObstacle)
            throw new Exception($"Player spawn position {spawnPointGrid} is on an obstacle");
        
        if(GameManager.Instance.IsSpreadingMode)
            tile.ChangeTileColor(playerNb);
        
        MovePlayerOnSpawnPoint(spawnPointGrid);
    }

    private void MovePlayerOnSpawnPoint(Vector2Int gridPos)
    {
        Vector3 worldPos = GridManagerStrategy.GridToWorldPosition(gridPos);
        var trans = transform;
        trans.position = new Vector3(worldPos.x, trans.position.y, worldPos.z);
        _currentTile = GameManager.Instance.GridManager.GetTileAtCoordinates(gridPos);
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
    }
    
    public void OnBomb(InputAction.CallbackContext ctx)
    {
        if (_stateMachine.CurrentState is JumpState) return;

        if (ctx.performed)
        {
            OnPlaceBomb?.Invoke();

            if (GameManager.Instance.BombManager.CreateBomb(transform.position,
                    this, _bombFusingStrategies[(int)BombFusingType], NextBombBombItems))
            {
                Animator.SetTrigger("DropBomb");
                OnPlaceBombSuccessfulChained?.Invoke(ItemType.ChainBombs);
                OnPlaceBombSuccessfulChained -= RemoveItemPopUp;
                OnPlaceBombSuccessful?.Invoke();
            }
        }
    }

    public void DisableInputActions() => _playerInput.actions.Disable();
    public void EnableInputActions() => _playerInput.actions.Enable();

    private void Update()
    {
        UpdateImmune();

        Tile tile = GetPlayerTile();
        if (tile != null) 
        {
            tile.StepOnTile(this);

            if (_currentTile != tile) 
            {
                _currentTile.StepOffTile(this);
                _currentTile.RemoveHighlight(PlayerNb);
            } 
            _currentTile = tile;
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

    public void OnHit(Vector2Int hitDirection, bool isHitFromSpikes = false)
    {
        //etant donne que hitDirection est un Vector2Int, y est z dans se cas
        if (IsImmune)
        {
            return;
        }
        
        if(isHitFromSpikes)
            SoundManager.Instance.OnEnterSpikes();
        else
            SoundManager.Instance.OnPlayerHitByBomb();

        Animator.SetTrigger("HitPlayer");
        Vector3 forceDirection = new Vector3(hitDirection.x,1,hitDirection.y);
        ApplyKnockback(forceDirection, knockbackForce);
        _stateMachine.Trigger(GameConstants.PLAYER_HIT_TRIGGER);
        IsImmune = true;
        _actualImmuneTimer = immuneTimer;

        ResetInventory();
    }

    public void ResetInventory() => playerItemsManager.ResetInventory();

    public void OnJump(Vector2Int jumpDirection) 
    {
        if (_stateMachine.CurrentState is not JumpState)
        {
            SoundManager.Instance.OnEnterTrampoline();
            _jumpVelocity = CalculateJumpForce(jumpDirection);
            _stateMachine.Trigger(GameConstants.PLAYER_JUMP_TRIGGER);
        }
    }

    public void OnPortal(Vector3 otherPortalPosition)
    {
            SoundManager.Instance.OnEnterPortal();
            _characterController.enabled = false;
            gameObject.transform.position = otherPortalPosition;
            _characterController.enabled = true;
    }

    private Vector3 CalculateJumpForce(Vector2Int jumpDirection)
    {
        //Formule pour trouver la vitesse initiale quand le sommet du saut est a (Obstacle.ObstacleHeight / 2) + 1 et au demi du trajet
        //position pour gravity 0.5*a*t^2
        float posForGravity = -(PLAYER_GRAVITY / 2) * Mathf.Pow(_airStateDuration / 2, 2);

        //position pour apogee du saut
        float jumpHeight = (Obstacle.ObstacleHeight) + JUMP_HEIGHT_OFFSET;

        float velocityY = (posForGravity + jumpHeight) / (_airStateDuration / 2);

        float velocityX = (Tile.TileLength * JUMP_NUMBER_OF_TILES) /(_airStateDuration);
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
    
    public void FlickerPlayerOnHit(float elapsedT) => SetRenderersVisible(Mathf.Sin(elapsedT * hitFlickerFrequency) > 0);

    private void SetRendererVisible() => SetRenderersVisible(true);

    public bool IsMoving() => _moveInput.sqrMagnitude > 0.01f;

    public void UpdateMovement()
    {
        Vector2 curMoveInput = _moveInput.normalized;

        float boost = CheckIfOnOwnColor() ? GameManager.Instance.ColorBoost : (CheckIfOnEnemyTerritory() ? GameManager.Instance.ColorDebuff : 1);

        boost *= GameManager.Instance.IsBonusSpeed ? _elimsSpeedBoost : 1;

        Vector3 move = new Vector3(curMoveInput.y, 0, -curMoveInput.x) * (speed * boost);
        float tempMove = ApplyGravity(ref _verticalVelocity);

        UpdatePlayerYRotation(curMoveInput);
        _characterController.enabled = false;
        _characterController.transform.position = transform.position;
        _characterController.enabled = true;
        _characterController.Move(move * Time.deltaTime);
        _characterController.Move(Vector3.down * Math.Abs(tempMove));
        
        OnMoveFunctionCalled?.Invoke();
    }

    public void UpdatePlayerYRotation(Vector2 moveInput) => transform.rotation = IsMoving() ? Quaternion.Euler(0, Mathf.Atan2(moveInput.y, -moveInput.x) * Mathf.Rad2Deg, 0) : transform.rotation;

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
            tempMove = (0.5f * PLAYER_GRAVITY * Time.deltaTime * Time.deltaTime) + (currentVerticalVelocity * Time.deltaTime);
            currentVerticalVelocity += PLAYER_GRAVITY * Time.deltaTime;
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
        GameManager.Instance.RemoveItemFromGrid(item);
        Destroy(other.gameObject);
        
        SoundManager.Instance.OnPickupItem();
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!other.tag.Equals("Bomb") || !other.transform.parent.TryGetComponent(out Bomb bomb) || bomb.HasColliderBeenRestored) return;
        bomb.RestoreColliderLayer();
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

    private void InitializeStateMachine()
    {
        _stateMachine = new StateMachine();
        _idleState = new IdleState(_stateMachine, this);
        _hitState = new HitState(_stateMachine, this);
        _runState = new RunState(_stateMachine, this);
        _jumpState = new JumpState(_stateMachine, this);


        _stateMachine.AddTransition<IdleState>(GameConstants.PLAYER_RUN_TRIGGER, _runState);
        _stateMachine.AddTransition<IdleState>(GameConstants.PLAYER_JUMP_TRIGGER, _jumpState);
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
        _playerInput = GetComponent<PlayerInput>();
        CachePlayerRenderers();
    }

    private void CachePlayerRenderers()
    {
        List<Renderer> playerRenderers = new();

        foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
        {
            playerRenderers.Add(renderer);
        }

        _renderers = playerRenderers.ToArray();
        _rendererDefaultStates = new bool[_renderers.Length];

        for (int i = 0; i < _renderers.Length; i++)
        {
            _rendererDefaultStates[i] = _renderers[i].enabled;
        }
    }

    private void SetRenderersVisible(bool isVisible)
    {
        if (_renderers == null)
        {
            return;
        }

        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] == null)
            {
                continue;
            }

            _renderers[i].enabled = _rendererDefaultStates[i] && isVisible;
        }
    }

    public void ActivatePaintbrushEffect()
    {
        if (_renderers == null)
        {
            return;
        }

        Material playerMaterial = GameManager.Instance.paintBrushEffect;

        for (int i = 0; i < _renderers.Length; ++i)
        {
            Material[] mats = _renderers[i].materials;
            for (int j = 0; j < mats.Length; ++j)
            {
                mats[j] = playerMaterial;
            }
            
            _renderers[i].materials = mats;
        }
    }

    public void ResetPlayerTexture()
    {
        if (_renderers == null)
        {
            return;
        }

        for (int i = 0; i < _renderers.Length && i < _initialMats.Count; ++i)
        {
            _renderers[i].materials = _initialMats[i];
        }
    }
    public void SetPlayerTexture()
    {
        if (_renderers == null)
        {
            return;
        }

        Material playerMaterial = playerMaterials[(int)playerNb - 1];

        for (int i = 0; i < _renderers.Length; ++i)
        {
            Material[] mats = _renderers[i].materials;
            for (int j = 0; j < mats.Length; ++j)
            {
                if (mats[j].name.Contains("PlayerTextureGrid"))
                {
                    mats[j] = playerMaterial;
                }
            }
            
            if(_initialMats.Count <= i) _initialMats.Add(mats);
            _renderers[i].materials = mats;
        }
    }

    private void ConfigurePlayers()
    {
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerNb = (PlayerEnum) playerInput.playerIndex + 1;
        }
        else
        {
            throw new Exception("There's no active player input");
        }
        
        playerColor = PlayerColorDict[playerNb];
        SetPlayerTexture();
    }

    public void DisplayPopUp(ItemType itemType, Sprite iconSprite)
    {
        itemTextPopUpText.text = itemType.ToString().AddSpacesBeforeCaps().ToUpper();
        AddIcon(itemType, iconSprite);
        
        if (_popUpCoroutine != null)
            StopCoroutine(_popUpCoroutine);
        _popUpCoroutine = StartCoroutine(DisplayPopUpCoroutine());
    }
    
    IEnumerator DisplayPopUpCoroutine()
    {
        itemTextPopUpBackground.gameObject.SetActive(true);
        yield return new WaitForSeconds(_popUpDuration);
        itemTextPopUpBackground.gameObject.SetActive(false);
    }
    
    private void AddIcon(ItemType itemType, Sprite sprite)
    {
        if (_activeIcons.ContainsKey(itemType))
            return;

        var icon = Instantiate(itemIconPrefab, itemIconsContainer);
        icon.GetComponentInChildren<Image>().sprite = sprite;

        _activeIcons[itemType] = icon;
    }
    
    public void RemoveItemPopUp(ItemType itemType)
    {
        if (!_activeIcons.TryGetValue(itemType, out var icon))
            return;

        _activeIcons.Remove(itemType);
        Destroy(icon.gameObject);
    }
}

public enum PlayerEnum
{
    None = 0,
    Player1 = 1,
    Player2 = 2,
    Player3 = 3,
    Player4 = 4
    //Do not forget to add the new player enums in the switch case of LobbyManager and modifying GameConstants.NB_PLAYERS
}

public enum BombFusingType
{
    None = 0,
    Target = 1
}

[Flags]
public enum BombItems
{
    None = 0,
    ChainedBombs = 1,
    FreezeBombs = 2
}
