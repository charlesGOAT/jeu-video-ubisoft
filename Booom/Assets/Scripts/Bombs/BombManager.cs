using System.Collections.Generic;
using UnityEngine;

public delegate void PaintbrushActivated(int layer);
public delegate void PaintbrushDeactivated(int layer);


public class BombManager : MonoBehaviour
{
    [SerializeField]
    private Bomb[] bombPrefabs;

    // Track each Player's bomb cooldown
    private readonly Dictionary<PlayerEnum, float> _nextBombTime = new (GameConstants.NB_PLAYERS);

    public List<int> LayersToExclude = new List<int>();
    
    public event PaintbrushActivated OnPaintbrushActivated;
    public event PaintbrushDeactivated OnPaintbrushDeactivated;
    
    protected virtual void Awake()
    {
        if (bombPrefabs == null)
        {
            Debug.LogError("Bomb prefabs shouldn't be empty");
            enabled = false;
        }

        for (int i = 1; i <= GameConstants.NB_PLAYERS; i++)
        {
            _nextBombTime.Add((PlayerEnum)i, 0f);
        }
    }

    public virtual bool CreateBomb(in Vector3 position, in Player player,in BombFusingStrategy bombStrat, in BombItems bombItems)
    {
        PlayerEnum playerEnum = player.PlayerNb;
        Vector3 bombHeightOffset = Vector3.up * 0.5f;

        if (Time.time < _nextBombTime[playerEnum])
        {
            return false;
        }

        Vector3 bombHeight = Vector3.up * position.y + bombHeightOffset;
        Vector2Int gridCoordinates = GridManagerStrategy.WorldToGridCoordinates(position);
        Tile tile = GameManager.Instance.GridManager.GetTileAtCoordinates(gridCoordinates);

        if (tile == null || tile.IsObstacle || Bomb.IsBombAt(gridCoordinates))
        {
            return false;
        }
        
        player.Animator.SetTrigger("DropBomb");

        BombEnum bombType = GameManager.Instance.EventManager.CurrentBombType;
        int intBombType = (int)bombType - 1;
        
        Vector3 worldPosition = GridManagerStrategy.GridToWorldPosition(gridCoordinates, tile.transform.position.y);

        Bomb instantiatedBomb = Instantiate(bombPrefabs[intBombType], worldPosition + bombHeight, Quaternion.identity);
        instantiatedBomb.AssociatedPlayer = playerEnum;
        instantiatedBomb.BombFusingStrategy = bombStrat;
        instantiatedBomb.IsFreezeBomb = bombItems.HasFlag(BombItems.FreezeBombs);

        foreach (Player p in Player.ActivePlayers.Values)
        {
            if (p.GetPlayerTile() == tile)
            {
                instantiatedBomb.RemoveColliderLayer(p.gameObject.layer);
            }
        }
    
#if !UNITY_EDITOR
        instantiatedBomb.ConfigureValues();
#endif
        
        float timeToAdd = bombItems.HasFlag(BombItems.ChainedBombs) ? 0 : instantiatedBomb.GetTimer();
        _nextBombTime[playerEnum] = Time.time + timeToAdd;

        return true;
    }

    public void ActivatePaintBrush(int layer)
    {
        LayersToExclude.Add(layer);
        OnPaintbrushActivated?.Invoke(layer);
    }
    public void DeactivatePaintBrush(int layer)
    {
        LayersToExclude.Remove(layer);
        OnPaintbrushDeactivated?.Invoke(layer);
    }
}
