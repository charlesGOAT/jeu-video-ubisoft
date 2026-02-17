using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Mathematics;
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
        _ = spawnMode switch
        {
            SpawnMode.Fixed => SpawnFixed(isDropFromSky),
            SpawnMode.Random => SpawnRandom(isDropFromSky),
            SpawnMode.Strategic => SpawnStrategic(isDropFromSky),
            _ => Task.CompletedTask
        };
    }

    protected virtual async Task SpawnFixed(bool isDropFromSky)
    {
        float lastTimeSpawned = Time.time;
        var gen = new System.Random();
        while (true)
        {
           await WaitForSpawnCond(lastTimeSpawned);
           var pos = GetRandomTilePos(fixedPosList, gen);

           if (isDropFromSky) await ManageShadow(pos);

           InstantiateItem(pos);
           lastTimeSpawned = Time.time;
        }
    }
    
    protected virtual async Task SpawnRandom(bool isDropFromSky)
    {
        float lastTimeSpawned = Time.time;
        while (true)
        { 
            await WaitForSpawnCond(lastTimeSpawned);
            Vector3 pos = GameManager.Instance.GridManager.GetRandomPosOnGrid();
            
            if (isDropFromSky) await ManageShadow(pos);

            InstantiateItem(pos);
            lastTimeSpawned = Time.time;
        }
    }
    
    protected virtual async Task SpawnStrategic(bool isDropFromSky)
    {
        float lastTimeSpawned = Time.time;
        var gen = new System.Random();
        while (true)
        {
            await WaitForSpawnCond(lastTimeSpawned);
            PlayerEnum player = GameManager.Instance.GridManager.FindPlayerWithMostGround();
            var playerTiles = GameManager.Instance.GridManager.GetPlayerTiles(player);
            Vector3 pos = GetRandomTilePos(playerTiles.ToList(), gen);
            
            if (isDropFromSky) await ManageShadow(pos);

            InstantiateItem(pos);
            lastTimeSpawned = Time.time;
        }
    }

    protected async Task WaitForSpawnCond(float lastSpawnTime)
    { 
        // todo : do we reset lastSpawn time on pickup instead ? 
        while (Time.time - lastSpawnTime < timeBetweenSpawns || NbItemsOnMap >= maxItems)
        {
            await Task.Yield();
        }
    }

    protected Vector3 GetRandomTilePos(List<Vector2Int> listPos, in System.Random random)
    {
        int index = random.Next(0, listPos.Count);
        return GridManagerStategy.GridToWorldPosition(listPos[index]);
    }

    protected async Task ManageShadow(Vector3 pos)
    { 
        GameObject shadowTemp = Instantiate(shadow, pos, Quaternion.identity);
        await Task.Delay(2000);  // todo tweak
        Destroy(shadowTemp);
    }

    protected void InstantiateItem(in Vector3 pos)
    {
        Instantiate(itemPrefab, pos, quaternion.identity);
        NbItemsOnMap++;
    }
}