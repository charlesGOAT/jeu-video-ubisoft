using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Tile : MonoBehaviour
{
    public Vector2Int TileCoordinates { get; private set; }

    public bool IsFrozen = false;

    public virtual bool IsObstacle => false;

    public static float TileLength { get; private set; }

    private Renderer _tileRenderer;

    public PlayerEnum CurrentTileOwner { get; private set; } = PlayerEnum.None;

    private List<PlayerEnum> _currentPlayersOnTile = new List<PlayerEnum>(GameConstants.NB_PLAYERS);

    private Color _neutralColor;

    private Material _highlightMat;

    private readonly Color _colorAdjust = new Color(75f/255f, 75f/255f, 75f/255f);

    public bool IsSpawn { get; set; }

    protected virtual void Awake()
    {
        if (TileLength == 0)
        {
            TileLength = transform.GetChild(0).localScale.x;
        }

        _tileRenderer = GetComponentInChildren<Renderer>();
        InitializeTileCoordinates();
    }

    protected virtual void Start()
    {
        _neutralColor = _tileRenderer.material.color;
        IsSpawn = GameManager.Instance.GridManager.playerSpawnPoints.Contains(TileCoordinates);
        _highlightMat = new Material(GameManager.Instance.highlightMat);
    }

    public virtual void ChangeTileColor(PlayerEnum newOwner)
    {
        if (CurrentTileOwner != newOwner && (!IsSpawn || _tileRenderer.material.color == _neutralColor) && !IsFrozen)
        {
            GameManager.Instance.ScoreManager.LoseTile(CurrentTileOwner, TileCoordinates);
            GameManager.Instance.ScoreManager.AcquireNewTile(newOwner, TileCoordinates);

            _tileRenderer.material.color = newOwner != PlayerEnum.None ? Player.PlayerColorDict[newOwner] : _neutralColor;
            CurrentTileOwner = newOwner;
        }
    }

    public virtual void StepOnTile(Player player)
    {
        HighlightTile(player.PlayerNb);
    }

    public void InitializeTileCoordinates()
    {
        TileCoordinates = GridManagerStrategy.WorldToGridCoordinates(transform.position);
    }

    private IEnumerator FreezeTileCoroutine()
    {
        IsFrozen = true;
        AddSnowflakeMaterial();
        yield return new WaitForSeconds(GameManager.Instance.FrozenTileDuration);
        IsFrozen = false;
        RemoveSnowflakeMaterial();
    }

    public void FreezeTile()
    {
        StartCoroutine(FreezeTileCoroutine());
    }

    private void ChangeTileMaterial(int matIndex, in Material mat)
    {
        Material[] materials = _tileRenderer.materials;
        materials[matIndex] = mat;
        _tileRenderer.materials = materials;
    }


    private void AddSnowflakeMaterial()
    {
        ChangeTileMaterial(2, GameManager.Instance.snowflakeMaterial);
    }
    
    private void RemoveSnowflakeMaterial()
    {
        ChangeTileMaterial(2, GameManager.Instance.transparentMat);
    }

    private void HighlightTile(PlayerEnum player, bool mutliplePlayers = false)
    {
        if (_currentPlayersOnTile.Count == 0 || mutliplePlayers)
        {
            Color newColor = _tileRenderer.material.color;
            if (GameManager.Instance.HighlightOwnColor)
            {
                newColor = newColor == Player.PlayerColorDict[player]
                    ? Player.PlayerColorDict[player] + _colorAdjust
                    : Player.PlayerColorDict[player];
            }
            else newColor += _colorAdjust;
                
            _highlightMat.SetColor("_BorderColor", newColor);
            ChangeTileMaterial(3, _highlightMat);
        }
        
        if (_currentPlayersOnTile.Contains(player)) return;
        _currentPlayersOnTile.Add(player);
    }
    
    public void RemoveHighlight(PlayerEnum player)
    {
        _currentPlayersOnTile.Remove(player);

        if(_currentPlayersOnTile.Count == 0)
            ChangeTileMaterial(3, GameManager.Instance.transparentMat);
        else
            HighlightTile(_currentPlayersOnTile[0], true);
    }
}
