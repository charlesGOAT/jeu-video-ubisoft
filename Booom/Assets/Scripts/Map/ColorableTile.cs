using UnityEngine;

public class ColorableTile : Tile
{
    private Renderer _tileRenderer;
    private Color _neutralColor;
    public PlayerEnum CurrentTileOwner { get; private set; } = PlayerEnum.None;
    public override bool IsObstacle => false;

    public void ChangeTileColor(PlayerEnum newOwner)
    {
        if (CurrentTileOwner != newOwner)
        {
            bool isNoPlayer = newOwner == PlayerEnum.None;

            if (CurrentTileOwner != PlayerEnum.None)
                GameManager.Instance.GridManager.tilesPerPlayer[(int)CurrentTileOwner - 1]--;

            if (!isNoPlayer)
                GameManager.Instance.GridManager.tilesPerPlayer[(int)newOwner - 1]++;

            _tileRenderer.material.color = !isNoPlayer ? Player.PlayerColorDict[newOwner] : _neutralColor;
            CurrentTileOwner = newOwner;
        }
    }

    void Awake()
    {
        base.Awake();
        _tileRenderer = GetComponentInChildren<Renderer>();
        _neutralColor = _tileRenderer.material.color;
    }

    public override void OnExplosion(PlayerEnum newOwner)
    {
        ChangeTileColor(newOwner);
    }
}
