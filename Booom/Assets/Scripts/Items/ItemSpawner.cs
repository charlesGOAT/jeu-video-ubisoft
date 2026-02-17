using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
           var pos = GetRandomTilePos(fixedPosList, gen);
    
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
            Vector3 pos = GameManager.Instance.GridManager.GetRandomPosOnGrid();
            
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
            PlayerEnum player = GameManager.Instance.GridManager.FindPlayerWithMostGround();
            var playerTiles = GameManager.Instance.GridManager.GetPlayerTiles(player);
            Vector3 pos = GetRandomTilePos(playerTiles.ToList(), gen);
            
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

    protected Vector3 GetRandomTilePos(List<Vector2Int> listPos, in System.Random random)
    {
        int index = random.Next(0, listPos.Count);
        return GridManagerStategy.GridToWorldPosition(listPos[index]);
    }

    protected IEnumerator ManageShadow(Vector3 pos)
    { 
        GameObject shadowTemp = Instantiate(shadow, pos, Quaternion.identity);
        yield return new WaitForSeconds(2f);  // todo tweak
        Destroy(shadowTemp);
    }

    protected void InstantiateItem(in Vector3 pos)
    {
        Instantiate(itemPrefab, pos, quaternion.identity);
        NbItemsOnMap++;
    }
}