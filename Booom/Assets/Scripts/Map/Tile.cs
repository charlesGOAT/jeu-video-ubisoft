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
        if (TileLength == 0)
        {
            TileLength = transform.GetChild(0).localScale.x;
        }

        _tileRenderer = GetComponentInChildren<Renderer>();
        InitializeTileCoordinates();

        _neutralColor = _tileRenderer.material.color;
    }

    protected virtual void Start()
    {
        IsSpawn = GameManager.Instance.GridManager.playerSpawnPoints.Contains(TileCoordinates);
        RemoveSnowflakeMaterial(); // because unity editor is broken yay
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

    public IEnumerator FreezeTile()
    {
        IsFrozen = true;
        AddSnowflakeMaterial();
        yield return new WaitForSeconds(GameManager.Instance.FrozenTileDuration);
        IsFrozen = false;
        RemoveSnowflakeMaterial();
    }

    private void AddSnowflakeMaterial()
    {
        Material[] materials = _tileRenderer.materials;
        materials[1] = GameManager.Instance.snowflakeMaterial;
        _tileRenderer.materials = materials;
    }
    
    private void RemoveSnowflakeMaterial()
    {
        Material[] materials = _tileRenderer.materials;
        materials[1] = GameManager.Instance.cadreMat;
        _tileRenderer.materials = materials;
    }
}
