using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    protected static readonly HashSet<Vector2Int> ActiveBombs = new HashSet<Vector2Int>();

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
        StartCoroutine(Pulse());
    }

    private IEnumerator Pulse()
    {
        float elapsed = 0f;
        while (elapsed < timer)
        {
            DoPulseMath(ref elapsed);
            yield return null;
        }
        Explode();
    }

    private IEnumerator PulseNoExplosion()
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
        StartCoroutine(PulseNoExplosion());
    }

    public void Explode()
    {
        PaintTiles();
        Destroy(gameObject);
    }

    protected virtual void PaintTiles()
    {
        Tile bombTile = GameManager.Instance.GridManager.GetTileAtCoordinates(_bombCoordinates);
        if (bombTile == null) return;

        PlayerEnum currentOwner = bombTile.CurrentTileOwner;
        PlayerEnum newTileOwner = GameManager.Instance.isSpreadingMode ? currentOwner : AssociatedPlayer;

        PaintTile(_bombCoordinates, Vector2Int.zero, newTileOwner);

        foreach (Vector2Int direction in _directions)
        {
            PaintTilesForDirection(_bombCoordinates + direction, direction, ExplosionRange, newTileOwner);
        }
    }

    private void PaintTilesForDirection(Vector2Int bombCoordinates, Vector2Int direction, int range, PlayerEnum newTileOwner)
    {
        if (range < 0) return;

        for (int rangeCounter = 0; rangeCounter < range; ++rangeCounter)
        {
            Tile tile = GameManager.Instance.GridManager.GetTileAtCoordinates(bombCoordinates + direction * rangeCounter);

            if (tile is Portal portalTile)
            {
                int tilesRemaining = range - rangeCounter;
                PaintTilesForDirectionUsingPortal(portalTile.GetOtherPortalPosition() + direction, direction, tilesRemaining, newTileOwner);
                return;
            }

            if (!PaintTile(bombCoordinates, direction * rangeCounter, newTileOwner))
            {
                return;
            }
        }
    }

    private bool PaintTile(in Vector2Int bombCoordinates, in Vector2Int direction, PlayerEnum newTileOwner)
    {
        Tile tile = GameManager.Instance.GridManager.GetTileAtCoordinates(bombCoordinates + direction);

        if (tile == null || (tile.IsObstacle && !IsTransparentBomb))
        {
            return false;
        }

        tile.ChangeTileColor(newTileOwner);
        HitPlayers(bombCoordinates + direction, bombCoordinates);
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

    private void PaintTilesForDirectionUsingPortal(Vector2Int bombCoordinates, Vector2Int direction, int range, PlayerEnum tileOwner)
    {
        if (range < 0 || GameManager.Instance.GridManager.GetTileAtCoordinates(bombCoordinates) == null) return;

        for (int rangeCounter = 0; rangeCounter <= range; ++rangeCounter)
        {
            Tile tile = GameManager.Instance.GridManager.GetTileAtCoordinates(bombCoordinates);

            if (tile is Portal portalTile)
            {
                int tilesRemaining = range - rangeCounter;
                PaintTilesForDirectionUsingPortal(portalTile.GetOtherPortalPosition() + direction, direction, tilesRemaining, tileOwner);
                return;
            }

            if (!PaintTile(bombCoordinates, direction, tileOwner))
            {
                return;
            }

            bombCoordinates += direction;
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
    FastBomb = 2,
    SplashBomb = 3
}
