using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int TileCoordinates { get; private set; }

    private Renderer _tileRenderer;

    public PlayerEnum CurrentTileOwner { get; private set; } = PlayerEnum.None;

    private Color _neutralColor;
    
    public void ChangeTileColor(PlayerEnum newOwner) 
    {
        _tileRenderer.material.color = newOwner != PlayerEnum.None ? Player.PlayerColorDict[newOwner] : _neutralColor; 
        CurrentTileOwner = newOwner;
    }

    void Awake()
    {
        _tileRenderer = GetComponentInChildren<Renderer>();
        TileCoordinates = GridManagerStategy.WorldToGridCoordinates(transform.position);

        _neutralColor = _tileRenderer.material.color;
    }

}