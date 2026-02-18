using System;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int TileCoordinates { get; private set; }

    [SerializeField]
    public bool isObstacle = false;

    private Renderer _tileRenderer;

    public PlayerEnum CurrentTileOwner { get; private set; } = PlayerEnum.None;

    private Color _neutralColor;
    
    public void ChangeTileColor(PlayerEnum newOwner) 
    {
        if (CurrentTileOwner != newOwner)
        {
            bool isNoPlayer = newOwner == PlayerEnum.None;
            
            GameManager.Instance.ScoreManager.LoseTile(CurrentTileOwner, TileCoordinates);
            GameManager.Instance.ScoreManager.AcquireNewTile(newOwner, TileCoordinates);
            
            _tileRenderer.material.color = !isNoPlayer ? Player.PlayerColorDict[newOwner] : _neutralColor;
            CurrentTileOwner = newOwner;
        }
    }

    void Awake()
    {
        _tileRenderer = GetComponentInChildren<Renderer>();
        TileCoordinates = GridManagerStrategy.WorldToGridCoordinates(transform.position);

        _neutralColor = _tileRenderer.material.color;
    }
}
