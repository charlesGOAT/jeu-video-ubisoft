using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int TileCoordinates { get; private set; }

    public virtual bool IsObstacle => false;

    public static float TileLength { get; private set; }

    private Renderer _tileRenderer;

    public PlayerEnum CurrentTileOwner { get; private set; } = PlayerEnum.None;

    private Color _neutralColor;

    protected virtual void Awake()
    {
        if (TileLength == 0)
        {
            TileLength = transform.GetChild(0).localScale.x;
        }

        _tileRenderer = GetComponentInChildren<Renderer>();
        InitializeTileCoordinates();

        _neutralColor = _tileRenderer.material.color;
    }

    public virtual void ChangeTileColor(PlayerEnum newOwner)
    {
        if (CurrentTileOwner != newOwner)
        {
            GameManager.Instance.ScoreManager.LoseTile(CurrentTileOwner, TileCoordinates);
            GameManager.Instance.ScoreManager.AcquireNewTile(newOwner, TileCoordinates);

            _tileRenderer.material.color = newOwner != PlayerEnum.None ? Player.PlayerColorDict[newOwner] : _neutralColor;
            CurrentTileOwner = newOwner;
        }
    }

    public virtual void StepOnTile(Player player)
    {
    }

    public void InitializeTileCoordinates()
    {
        TileCoordinates = GridManagerStrategy.WorldToGridCoordinates(transform.position);
    }
}
