using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public static readonly HashSet<Vector2Int> ActiveBombs = new HashSet<Vector2Int>();

    [SerializeField]
    protected float timer = 3.0f;
    public float Timer => timer;

    [SerializeField]
    private float pulseAmplitude = 0.2f;

    [SerializeField]
    private float pulseSpeed = 8f;

    [SerializeField]
    protected int explosionRange = 3;

    public int ExplosionRange => explosionRange;

    public BombFusingStrategy BombFusingStrategy = new();

    public bool IsTransparentBomb { private get; set; } = false;

    private readonly Vector2Int[] _directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    private Vector3 _initialScale;

    protected Vector2Int _bombCoordinates;

    public PlayerEnum AssociatedPlayer = PlayerEnum.None;

    private void Awake()
    {
        Transform trans = transform;

        _initialScale = trans.localScale;
        _bombCoordinates = GridManagerStrategy.WorldToGridCoordinates(trans.position);
        ActiveBombs.Add(_bombCoordinates);

        if (!GameManager.Instance.IsBonusSpeed && AssociatedPlayer != PlayerEnum.None)
        {
            explosionRange += Player.ActivePlayers[(int)AssociatedPlayer - 1].ElimsRangeBoost;
        }
    }

    protected virtual void Start()
    {
        BombFusingStrategy.Fuse(this);
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
        float elapsed = 0f;
        while (elapsed < timer)
        {
            DoPulseMath(ref elapsed);
            yield return null;
        }
        Explode();
    }

    private IEnumerator Pulse()
    {
        float elapsed = 0f;
        while (elapsed < timer)
        {
            DoPulseMath(ref elapsed);
            yield return null;
        }
    }

    private void DoPulseMath(ref float elapsed)
    {
        float pulse = 1f + (Mathf.Abs(Mathf.Sin(elapsed * pulseSpeed)) * pulseAmplitude);
        transform.localScale = _initialScale * pulse;
        elapsed += Time.deltaTime;
    }

    public void StartPulseCoroutine()
    {
        StartCoroutine(Pulse());
    }
    
    public void Explode()
    {
        SoundManager.Instance.OnBombExploded(this);
        PaintTiles();
        Destroy(gameObject);
    }

    protected virtual void PaintTiles()
    {
        Tile bombTile = GameManager.Instance.GridManager.GetTileAtCoordinates(_bombCoordinates);
        if (bombTile == null) return;

        PlayerEnum currentOwner = bombTile.CurrentTileOwner;
        PlayerEnum newTileOwner = GameManager.Instance.IsSpreadingMode ? currentOwner : AssociatedPlayer;

        PaintTile(bombTile, newTileOwner);

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
            
            if (!PaintTile(tile, newTileOwner))
            {
                if (IsTransparentBomb) 
                {
                    continue;
                }
                
                return;
            }
            
            HitPlayers(explosionCoords, direction);
        }
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
        ActiveBombs.Remove(_bombCoordinates);
        _bombCoordinates = newBombCoordinates;
        ActiveBombs.Add(_bombCoordinates);
    }

    protected virtual void OnDestroy()
    {
        ActiveBombs.Remove(_bombCoordinates);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.tag.Equals("Player") ||
            !collision.collider.gameObject.TryGetComponent(out Player player) ||
            player.PlayerNb == AssociatedPlayer) return;
            
        BombFusingStrategy.OnCollision(this);
    }
}

public enum BombEnum
{
    None = 0,
    NormalBomb = 1,
    FastBomb = 2
}
