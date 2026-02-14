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
        _tileRenderer = GetComponentInChildren<Renderer>();
        TileCoordinates = GridManagerStategy.WorldToGridCoordinates(transform.position);

        _neutralColor = _tileRenderer.material.color;
    }

}
