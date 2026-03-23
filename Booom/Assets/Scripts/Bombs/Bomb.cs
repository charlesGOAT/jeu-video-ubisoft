using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnExplode();

public class Bomb : MonoBehaviour
{
    public static readonly HashSet<Vector2Int> ActiveBombs = new HashSet<Vector2Int>();

    private float _timer = 3f;

    [SerializeField]
    protected int explosionRange = 3;

    public int ExplosionRange => explosionRange;

    public BombFusingStrategy BombFusingStrategy = new();

    private BombAnimation _bombAnimation;
    private Tile _subscribedTile;

    public event OnExplode OnExplode;

    public bool IsTransparentBomb { private get; set; }
    public bool IsFreezeBomb { private get; set; }

    private Collider _colliderComp;

    private LayerMask _oldLayerMask;
    
    public bool HasColliderBeenRestored { get; private set; }

    private readonly Vector2Int[] _directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    protected Vector2Int _bombCoordinates;

    public PlayerEnum AssociatedPlayer = PlayerEnum.None;

    private void Awake()
    {
        Transform trans = transform;

        _bombCoordinates = GridManagerStrategy.WorldToGridCoordinates(trans.position);
        ActiveBombs.Add(_bombCoordinates);

        if (!GameManager.Instance.IsBonusSpeed && AssociatedPlayer != PlayerEnum.None)
        {
            explosionRange += Player.ActivePlayers[(int)AssociatedPlayer - 1].ElimsRangeBoost;
        }

        _bombAnimation = GetComponent<BombAnimation>();
        _bombAnimation.InitializeAnimation(GetTimer());

        _colliderComp = GetComponent<Collider>();

        GameManager.Instance.BombManager.OnPaintbrushActivated += OnPaintbrushActivated;
        GameManager.Instance.BombManager.OnPaintbrushDeactivated += OnPaintbrushDeactivated;

        foreach (int layer in GameManager.Instance.BombManager.LayersToExclude)
        {
            OnPaintbrushActivated(layer);
        }
    }

    protected virtual void Start()
    {
        SubscribeToCurrentTileColor();
        BombFusingStrategy.Fuse(this);
    }

    public virtual float GetTimer()
    {
        return _timer;
    }

    public virtual void ConfigureValues()
    {
        _timer = GameManager.Instance.RuntimeConfig.NormalBombTimer;
    }

    public static bool IsBombAt(Vector2Int gridCoordinates)
    {
        return ActiveBombs.Contains(gridCoordinates);
    }

    public void StartBombCountDown()
    {
        StartCoroutine(CountdownAndExplode());
    }

    private IEnumerator CountdownAndExplode()
    {
        yield return new WaitForSeconds(GetTimer());
        Explode();
    }

    public void Explode()
    {
        if(!IsFreezeBomb) SoundManager.Instance.OnBombExploded();
        else SoundManager.Instance.OnDefenseBombExploded();
        PaintTiles();
        NotifyExplosionSubscribers();
        Destroy(gameObject);
    }

    protected virtual void PaintTiles()
    {
        Tile bombTile = GameManager.Instance.GridManager.GetTileAtCoordinates(_bombCoordinates);
        if (bombTile == null) return;

        PlayerEnum currentOwner = bombTile.CurrentTileOwner;
        PlayerEnum newTileOwner = GameManager.Instance.IsSpreadingMode ? currentOwner : AssociatedPlayer;

        ChoosePaintTile(bombTile, newTileOwner);
        HitPlayers(_bombCoordinates, Vector2Int.zero);

        foreach (Vector2Int direction in _directions)
        {
            PaintTilesForDirection(_bombCoordinates + direction, direction, explosionRange, newTileOwner);
        }
    }

    private void PaintTilesForDirection(Vector2Int bombCoordinates, Vector2Int direction, int range, PlayerEnum newTileOwner)
    {
        if (range <= 0) return;

        for (int rangeCounter = 0; rangeCounter < range; ++rangeCounter)
        {
            var explosionCoords = bombCoordinates + rangeCounter * direction;
            Tile tile = GameManager.Instance.GridManager.GetTileAtCoordinates(explosionCoords);

            if (tile is Portal portalTile)
            {
                int tilesRemaining = range - rangeCounter;
                PaintTilesForDirection(portalTile.GetOtherPortalPosition() + direction, direction, tilesRemaining, newTileOwner);
                return;
            }
            
            if (!ChoosePaintTile(tile, newTileOwner))
            {
                return;
            }
            
            HitPlayers(explosionCoords, direction);
        }
    }

    private bool ChoosePaintTile(in Tile tile, PlayerEnum newTileOwner)
    {
        if (!IsFreezeBomb)
            return PaintTile(tile, newTileOwner);
        
        if (!PaintTile(tile, newTileOwner)) return false;
        tile.FreezeTile();
        return true;
    }

    private bool PaintTile(in Tile tile, PlayerEnum newTileOwner)
    {
        if (tile == null || tile.IsObstacle)
        {
            return false;
        }

        tile.ChangeTileColor(newTileOwner);
        return true;
    }

    protected void HitPlayers(Vector2Int tileCoordinates, Vector2Int hitDirection)
    {
        foreach (Player player in Player.ActivePlayers)
        {
            Tile playerTile = player.GetPlayerTile();
            if (playerTile != null && playerTile.TileCoordinates == tileCoordinates)
            {
                if (player.PlayerNb != AssociatedPlayer)
                {
                    GameManager.Instance.ScoreManager.NewElimination(AssociatedPlayer);
                }

                player.OnHit(hitDirection);
            }
        }
    }

    public void SetBombCoordinates(Vector2Int newBombCoordinates)
    {
        UnsubscribeFromCurrentTileColor();
        ActiveBombs.Remove(_bombCoordinates);
        _bombCoordinates = newBombCoordinates;
        ActiveBombs.Add(_bombCoordinates);
        SubscribeToCurrentTileColor();
    }

    public void NotifyExplosionSubscribers() 
    {
        OnExplode?.Invoke();
    }

    protected virtual void OnDestroy()
    {
        UnsubscribeFromCurrentTileColor();
        ActiveBombs.Remove(_bombCoordinates);
        GameManager.Instance.BombManager.OnPaintbrushActivated -= OnPaintbrushActivated;
        GameManager.Instance.BombManager.OnPaintbrushDeactivated -= OnPaintbrushDeactivated;
    }

    private void SubscribeToCurrentTileColor()
    {
        Tile tile = GameManager.Instance.GridManager.GetTileAtCoordinates(_bombCoordinates);
        if (tile == null || _bombAnimation == null)
        {
            return;
        }

        _subscribedTile = tile;
        _subscribedTile.OnTileColorChanged += OnTileColorChanged;
        OnTileColorChanged(GetCurrentTileColor(tile));
    }

    private void UnsubscribeFromCurrentTileColor()
    {
        if (_subscribedTile == null)
        {
            return;
        }

        _subscribedTile.OnTileColorChanged -= OnTileColorChanged;
        _subscribedTile = null;
    }

    private void OnTileColorChanged(Color tileColor)
    {
        _bombAnimation.SetBombColor(tileColor);
    }

    private Color GetCurrentTileColor(Tile tile)
    {
        return tile.CurrentTileOwner != PlayerEnum.None ? Player.PlayerColorDict[tile.CurrentTileOwner] : GetNeutralTileColor(tile);
    }

    private static Color GetNeutralTileColor(Tile tile)
    {
        Renderer tileRenderer = tile.GetComponentInChildren<Renderer>();
        return tileRenderer != null ? tileRenderer.material.color : Color.white;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.tag.Equals("Player") ||
            !collision.collider.gameObject.TryGetComponent(out Player player) ||
            player.PlayerNb == AssociatedPlayer) return;
            
        BombFusingStrategy.OnCollision(this);
    }

    private void OnPaintbrushActivated(int layer)
    {
        if(_colliderComp != null)
            _colliderComp.excludeLayers = _colliderComp.excludeLayers.value | (1 << layer);
    }
    
    private void OnPaintbrushDeactivated(int layer)
    {
        if(_colliderComp != null)
            _colliderComp.excludeLayers = _colliderComp.excludeLayers.value & ~(1 << layer);
    }
    
    public void RemoveColliderLayer(GameObject player)
    {
        var ogLayerMask = _colliderComp.excludeLayers.value;
        var newLayer = ogLayerMask | (1 << player.layer);
        _colliderComp.excludeLayers = newLayer;
    }
    
    public void RestoreColliderLayer()
    {
        _colliderComp.excludeLayers = _oldLayerMask;
        HasColliderBeenRestored = true;
    }
}

public enum BombEnum
{
    None = 0,
    NormalBomb = 1,
    FastBomb = 2
}
