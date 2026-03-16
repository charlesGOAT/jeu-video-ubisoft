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

    private Color _neutralColor;

    public bool IsSpawn { get; set; }

    protected virtual void Awake()
    {
        if (TileLength == 0 && transform.childCount > 0)
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

    private void AddSnowflakeMaterial()
    {
        Material[] materials = _tileRenderer.materials;
        materials[2] = GameManager.Instance.snowflakeMaterial;
        _tileRenderer.materials = materials;
    }
    
    private void RemoveSnowflakeMaterial()
    {
        Material[] materials = _tileRenderer.materials;
        materials[2] = GameManager.Instance.transparentMat;
        _tileRenderer.materials = materials;
    }
}
