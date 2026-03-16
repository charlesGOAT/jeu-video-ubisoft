using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void PaintbrushActivated(int layer);
public delegate void PaintbrushDeactivated(int layer);


public class BombManager : MonoBehaviour
{
    [SerializeField]
    private Bomb[] bombPrefabs;

    [SerializeField]
    private bool ShouldBombCollideWithPlayers = true;

    // Track each Player's bomb cooldown
    private readonly Dictionary<PlayerEnum, float> _nextBombTime = new (GameConstants.NB_PLAYERS);
    private readonly Dictionary<PlayerEnum, List<Bomb>> _chainedBombsPerPlayer = new (GameConstants.NB_PLAYERS);

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
            _chainedBombsPerPlayer.Add((PlayerEnum)i, new());
        }
    }

    private void Start()
    {
#if !UNITY_EDITOR
        ShouldBombCollideWithPlayers = GameManager.Instance.RuntimeConfig.ShouldBombCollideWithPlayers;
#endif

        if (!ShouldBombCollideWithPlayers)
        {
            InitializeBombCollisionLayers();
        }
    }

    private void InitializeBombCollisionLayers()
    {
        foreach (Bomb bomb in bombPrefabs)
        {
            Collider bombCol = bomb.GetComponent<Collider>();

            foreach (var layer in GameManager.Instance.CollisionLayers)
            {
                bombCol.excludeLayers |= layer;
            }
        }
    }
    
    public virtual bool CreateBomb(in Vector3 position, in Player player,in BombFusingStrategy bombStrat, bool isTransparentBomb = false, bool isFreezeBomb = false)
    {
        bool isChained = bombStrat is ChainedBombFusingStrategy;
        PlayerEnum playerEnum = player.PlayerNb;
        
        if (Time.time < _nextBombTime[playerEnum] && !isChained)
        {
            return false;
        }

        Vector3 bombHeight = Vector3.up * position.y;
        Vector2Int gridCoordinates = GridManagerStrategy.WorldToGridCoordinates(position);
        Tile tile = GameManager.Instance.GridManager.GetTileAtCoordinates(gridCoordinates);

        if (tile == null || tile.IsObstacle || Bomb.IsBombAt(gridCoordinates))
        {
            return false;
        }

        BombEnum bombType = GameManager.Instance.EventManager.CurrentBombType;
        int intBombType = (int)bombType - 1;
        
        Vector3 worldPosition = GridManagerStrategy.GridToWorldPosition(gridCoordinates, tile.transform.position.y);
        bombPrefabs[intBombType].AssociatedPlayer = playerEnum;

        Bomb instantiatedBomb = Instantiate(bombPrefabs[intBombType], worldPosition + bombHeight, Quaternion.identity);
        instantiatedBomb.BombFusingStrategy = bombStrat;
        instantiatedBomb.IsTransparentBomb = isTransparentBomb;
        instantiatedBomb.IsFreezeBomb = isFreezeBomb;

        if(ShouldBombCollideWithPlayers)
            StartCoroutine(ChangeColliderLayer(instantiatedBomb, player.gameObject));

        if (isChained)
            _chainedBombsPerPlayer[playerEnum].Add(instantiatedBomb);

        _nextBombTime[playerEnum] = Time.time + instantiatedBomb.Timer;

        return true;
    }

    public void ExplodeChainedBombs(PlayerEnum player)
    {
        foreach (Bomb bomb in _chainedBombsPerPlayer[player])
        {
            bomb.Explode();
        }
        
        _chainedBombsPerPlayer[player].Clear();
    }

    public bool HasChainedBombs(PlayerEnum player)
    {
        return _chainedBombsPerPlayer[player].Count != 0;
    }

    private IEnumerator ChangeColliderLayer(Bomb bomb, GameObject player)
    {
        var ogLayerMask = bomb.ColliderComp.excludeLayers.value;
        var newLayer = ogLayerMask | (1 << player.layer);
        bomb.ColliderComp.excludeLayers = newLayer;
        yield return new WaitForSeconds(1.0f);
        bomb.ColliderComp.excludeLayers = ogLayerMask;
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
