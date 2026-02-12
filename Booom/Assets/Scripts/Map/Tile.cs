using System;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int TileCoordinates { get; private set; }

    [SerializeField]
    public bool isObstacle = false;

    private Renderer _tileRenderer;

    public Color CurrentTileColor { get; private set; }

    private Color _neutralColor;
    
    public void ChangeTileColor(Color color) 
    {
        if (color != CurrentTileColor)
        {
            if (CurrentTileColor != _neutralColor)
            {
                PlayerEnum currentOwner = Player.PlayerColorDict[CurrentTileColor];
                GameManager.Instance.GridManager.tilesPerPlayer[(int)currentOwner - 1]--;
            }

            if (color != _neutralColor)
            {
                PlayerEnum newOwner = Player.PlayerColorDict[color];
                GameManager.Instance.GridManager.tilesPerPlayer[(int)newOwner - 1]++;
            }
            
            _tileRenderer.material.color = color;
            CurrentTileColor = color;
        }
    }

    void Awake()
    {
        _tileRenderer = GetComponentInChildren<Renderer>();
        TileCoordinates = GridManagerStategy.WorldToGridCoordinates(transform.position);

        _neutralColor = _tileRenderer.material.color;
        CurrentTileColor = _neutralColor;
    }

}
