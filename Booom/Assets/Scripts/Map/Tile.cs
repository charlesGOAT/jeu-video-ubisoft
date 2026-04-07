using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public delegate void OnTileColorChanged(Color newColor);

[RequireComponent(typeof(TileAnimation))]
public class Tile : MonoBehaviour
{
    public Vector2Int TileCoordinates { get; private set; }

    public bool IsFrozen = false;

    public virtual bool IsObstacle => false;

    public static float TileLength { get; private set; }

    private Renderer _tileRenderer;
    private TileAnimation _tileAnimation;

    [SerializeField] 
    private GameObject snowEffect;

    public PlayerEnum CurrentTileOwner { get; private set; } = PlayerEnum.None;

    private List<PlayerEnum> _currentPlayersOnTile = new List<PlayerEnum>(GameConstants.NB_PLAYERS);

    public Color NeutralColor { get; private set; }
    private Color _tileColor;

    private Material _highlightMat;
    private Material _blinkMat;

    private readonly Color _colorAdjust = new Color(100f/255f, 100f/255f, 100f/255f);
    private static readonly int TileColor = Shader.PropertyToID("_TileColor");

    public bool IsSpawn { get; set; }
    
    public event OnTileColorChanged OnTileColorChanged;

    protected virtual void Awake()
    {
        if (TileLength == 0 && transform.childCount > 0)
        {
            TileLength = transform.GetChild(0).localScale.x;
        }
        
        _tileRenderer = GetComponentInChildren<Renderer>();
        _tileAnimation = GetComponent<TileAnimation>();

        _tileAnimation.Initialize(_tileRenderer);
        InitializeTileCoordinates();
    }

    protected virtual void Start()
    {
        NeutralColor = _tileRenderer.material.GetColor("_TileColor");
        _tileColor = NeutralColor;
        IsSpawn = GameManager.Instance.GridManager.playerSpawnPoints.Contains(TileCoordinates);
        _highlightMat = new Material(GameManager.Instance.highlightMat);
        _blinkMat = GameManager.Instance.blinkMat;
    }

    public virtual void ChangeTileColor(PlayerEnum newOwner)
    {
        if (!IsSpawn || _tileColor == NeutralColor)
        {
            _tileColor = newOwner != PlayerEnum.None ? Player.PlayerColorDict[newOwner] : NeutralColor;

            OnTileColorChanged?.Invoke(_tileColor);
            if (CurrentTileOwner != newOwner && !IsFrozen)
            {
                GameManager.Instance.ScoreManager.LoseTile(CurrentTileOwner, TileCoordinates);
                GameManager.Instance.ScoreManager.AcquireNewTile(newOwner, TileCoordinates);
                _tileAnimation.AnimateTileColorChange(_tileColor);
                CurrentTileOwner = newOwner;
                _highlightMat.SetColor("_BorderColor", ClampColor(_tileColor - _colorAdjust));
            }
            else if (CurrentTileOwner == newOwner  && !IsFrozen && !IsSpawn)
            {
                _tileAnimation.AnimateExplosionFeedback(_tileColor);
            }
        }
        else
        {
            _tileAnimation.AnimateTileColorChange(_tileColor); // faire l'animation tout de même pour montrer une continuation
        }
    }

    public Color ClampColor(in Color color)
    {
        Color res = new();
        res.r = Mathf.Clamp(color.r, 0, 1);
        res.b = Mathf.Clamp(color.b, 0, 1);
        res.g = Mathf.Clamp(color.g, 0, 1);
        res.a = color.a;
        return res;
    }

    public virtual void StepOnTile(Player player)
    {
        if(player.IsUsingPaintbrush && CurrentTileOwner != player.PlayerNb) 
            ChangeTileColor(player.PlayerNb);
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

    protected virtual void ChangeTileMaterial(int matIndex, in Material mat)
    {
        Material[] materials = _tileRenderer.materials;
        materials[matIndex] = mat;
        _tileRenderer.materials = materials;
    }

    private void AddSnowflakeMaterial()
    {
        ChangeTileMaterial(3, GameManager.Instance.snowflakeMaterial);
        snowEffect.SetActive(true);
    }

    private void RemoveSnowflakeMaterial()
    {
        ChangeTileMaterial(3, GameManager.Instance.transparentMat);
        snowEffect.SetActive(false);
    }

    private void HighlightTile(PlayerEnum player, bool mutliplePlayers = false)
    {
        if (_currentPlayersOnTile.Count == 0 || mutliplePlayers)
        {
            Color newColor = _tileColor == NeutralColor ? _tileColor + _colorAdjust : _tileColor - _colorAdjust;
            newColor = ClampColor(newColor);

            _highlightMat.SetColor("_BorderColor", newColor);
            ChangeTileMaterial(4, _highlightMat);
        }

        if (_currentPlayersOnTile.Contains(player)) return;
        _currentPlayersOnTile.Add(player);
    }

    public void RemoveHighlight(PlayerEnum player)
    {
        _currentPlayersOnTile.Remove(player);

        if (_currentPlayersOnTile.Count == 0)
            ChangeTileMaterial(4, GameManager.Instance.transparentMat);
        else
            HighlightTile(_currentPlayersOnTile[0], true);
    }

    public virtual void StepOffTile(Player player)
    {
        RemoveHighlight(player.PlayerNb);
    }

    public void AddWinnerBlink(in PlayerEnum winner)
    {
        if (CurrentTileOwner != winner) return;
        _blinkMat.SetColor(TileColor, _tileColor);
        ChangeTileMaterial(2, _blinkMat);
    }
}
