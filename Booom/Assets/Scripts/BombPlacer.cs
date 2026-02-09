using UnityEngine;

public class BombPlacer : MonoBehaviour
{
    [SerializeField]
    private GridManager gridManager;
    [SerializeField]
    private GameObject bombPrefab;
    [SerializeField]
    private float cooldownSeconds = 3f; // time between drops
    [SerializeField]
    private float bombFuseSeconds = 3f; // time until bomb explodes
    [SerializeField]
    private KeyCode dropKey = KeyCode.Space;

    private float _nextDropTime;

    private void Start()
    {
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
        }
    }

    private void Update()
    {
        if (gridManager == null) return;
        if (!Input.GetKeyDown(dropKey)) return;
        if (Time.time < _nextDropTime) return;

        if (!gridManager.TryWorldToCell(transform.position, out Vector2Int cell)) return;
        if (!gridManager.TryOccupyCell(cell)) return;

        Vector3 spawnPosition = gridManager.GetCellCenter(cell);
        spawnPosition.y = transform.position.y;

        GameObject bombObject = InstantiateBomb(spawnPosition);
        Bomb bomb = bombObject.GetComponent<Bomb>();
        if (bomb == null)
        {
            bomb = bombObject.AddComponent<Bomb>();
        }

        bomb.Init(gridManager, cell, bombFuseSeconds);
        _nextDropTime = Time.time + cooldownSeconds;
    }

    private GameObject InstantiateBomb(Vector3 position)
    {
        if (bombPrefab != null)
        {
            return Instantiate(bombPrefab, position, Quaternion.identity);
        }

        GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        primitive.transform.position = position;
        primitive.name = "Bomb";
        return primitive;
    }
}
