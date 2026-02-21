using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField]
    private ItemType associatedItemType;
    [SerializeField]
    protected int maxItems = 0;
    [SerializeField]
    protected float timeBetweenSpawns = 0;
    [SerializeField]
    protected List<Vector2Int> fixedPosList;
    [SerializeField]
    protected Item itemPrefab;
    [SerializeField]
    protected GameObject shadow;
    public ItemType AssociatedItemType => associatedItemType;

    private void Awake()
    {
        // todo check that everything is set
    }

    public int NbItemsOnMap { get; set; } = 0;

    public void Spawn(SpawnMode spawnMode, bool isDropFromSky)
    {
        switch (spawnMode)
        {
            case SpawnMode.Fixed:
                StartCoroutine(SpawnFixed(isDropFromSky));
                break;
            case SpawnMode.Random:
                StartCoroutine(SpawnRandom(isDropFromSky));
                break;
            case SpawnMode.Strategic:
                StartCoroutine(SpawnStrategic(isDropFromSky));
                break;
        }
    }
    
    protected virtual IEnumerator SpawnFixed(bool isDropFromSky)
    {
        float lastTimeSpawned = Time.time;
        var gen = new System.Random();
        while (true)
        {
           yield return StartCoroutine(WaitForSpawnCond(lastTimeSpawned));
           var pos = GetRandomTilePos(fixedPosList.Where(pos => !GameManager.Instance.GridManager.IsItemAtPos(pos)), gen);
    
           if (isDropFromSky) yield return StartCoroutine(ManageShadow(pos));
    
           InstantiateItem(pos);
           lastTimeSpawned = Time.time;
        }
    }

    protected virtual IEnumerator SpawnRandom(bool isDropFromSky)
    {
        float lastTimeSpawned = Time.time;
        while (true)
        { 
            yield return StartCoroutine(WaitForSpawnCond(lastTimeSpawned));
            Vector3 pos = GameManager.Instance.GridManager.GetRandomPosOnGridWithNoItem();
            
            if (isDropFromSky) yield return StartCoroutine(ManageShadow(pos));

            InstantiateItem(pos);
            lastTimeSpawned = Time.time;
        }
    }
    
    protected virtual IEnumerator SpawnStrategic(bool isDropFromSky)
    {
        float lastTimeSpawned = Time.time;
        var gen = new System.Random();
        while (true)
        {
            yield return StartCoroutine(WaitForSpawnCond(lastTimeSpawned));
            PlayerEnum player = GameManager.Instance.ScoreManager.FindPlayerWithMostGround();
            var playerTiles = GameManager.Instance.GridManager.GetPlayerTilesWithNoItem(player);
            Vector3 pos = GetRandomTilePos(playerTiles, gen);
            
            if (isDropFromSky) yield return StartCoroutine(ManageShadow(pos));

            InstantiateItem(pos);
            lastTimeSpawned = Time.time;
        }
    }

    protected IEnumerator WaitForSpawnCond(float lastSpawnTime)
    { 
        // todo : do we reset lastSpawn time on pickup instead ? 
        while (Time.time - lastSpawnTime < timeBetweenSpawns || NbItemsOnMap >= maxItems)
        {
            yield return null;
        }
    }

    protected Vector3 GetRandomTilePos(IEnumerable<Vector2Int> listPos, in System.Random random)
    {
        var listP = listPos.ToArray();
        int index = random.Next(0, listP.Length);
        return GridManagerStrategy.GridToWorldPosition(listP[index]);
    }

    protected IEnumerator ManageShadow(Vector3 pos)
    { 
        GameObject shadowTemp = Instantiate(shadow, pos, Quaternion.identity);
        yield return new WaitForSeconds(2f);  // todo tweak
        Destroy(shadowTemp);
    }

    protected void InstantiateItem(in Vector3 pos)
    {
        Vector2Int posOnMap = GridManagerStrategy.WorldToGridCoordinates(pos);
        
        Item item = Instantiate(itemPrefab, pos, Quaternion.identity);
        item.posOnMap = posOnMap;
        
        GameManager.Instance.GridManager.AddItemOnGrid(item);
        NbItemsOnMap++;
    }
}