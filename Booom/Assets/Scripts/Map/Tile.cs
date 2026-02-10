using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int TileCoordinates { get; private set; }

    [SerializeField]
    public bool isObstacle = false;

    private Renderer _tileRenderer;

    public PlayerEnum CurrentTileOwner { get; private set; } = PlayerEnum.None;
    
    public void ChangeTileColor(Color color, PlayerEnum newOwner) 
    {
        _tileRenderer.material.color = color;
        CurrentTileOwner = newOwner;
    }

    void Awake()
    {
        _tileRenderer = GetComponentInChildren<Renderer>();
        TileCoordinates = GridManagerStategy.WorldToGridCoordinates(transform.position);
    }

}
