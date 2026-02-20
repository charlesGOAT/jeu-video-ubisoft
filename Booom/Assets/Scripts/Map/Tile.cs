using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int TileCoordinates { get; private set; }

    public virtual bool IsObstacle => false;

    public static float TileLength { get; private set; }

    private Renderer _tileRenderer;

    public PlayerEnum CurrentTileOwner { get; private set; } = PlayerEnum.None;

    private Color _neutralColor;
    
    public virtual void ChangeTileColor(PlayerEnum newOwner) 
    {
        if (CurrentTileOwner != newOwner)
        {
            bool isNoPlayer = newOwner == PlayerEnum.None;
            
            GameManager.Instance.GridManager.LoseTile(CurrentTileOwner, TileCoordinates);
            GameManager.Instance.GridManager.AquireNewTile(newOwner, TileCoordinates);
            
            _tileRenderer.material.color = !isNoPlayer ? Player.PlayerColorDict[newOwner] : _neutralColor;
            CurrentTileOwner = newOwner;
        }
    }

    public virtual void StepOnTile(Player player) 
    {
        //update vitesse ici?
    }

    protected virtual void Awake()
    {
        if (TileLength == 0) 
        {
            TileLength = transform.GetChild(0).localScale.x;
        }

        _tileRenderer = GetComponentInChildren<Renderer>();
        TileCoordinates = GridManagerStategy.WorldToGridCoordinates(transform.position);

        _neutralColor = _tileRenderer.material.color;
    }
}
