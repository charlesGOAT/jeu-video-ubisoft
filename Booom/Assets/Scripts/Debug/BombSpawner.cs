using UnityEngine;
using UnityEngine.InputSystem;

public class BombSpawner : MonoBehaviour
{
    [SerializeField]
    private Bomb bombPrefab;

    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private LayerMask tileLayerMask = ~0;

    [SerializeField]
    private float maxRayDistance = 100f;

    [SerializeField]
    private float spawnYOffset = 0.5f;

    [SerializeField]
    private Key triggerKey = Key.Space;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TrySpawnFromMouse();
        }

        if (Keyboard.current != null && Keyboard.current[triggerKey].wasPressedThisFrame)
        {
            TrySpawnFromScreenCenter();
        }
    }

    private void TrySpawnFromMouse()
    {
        if (bombPrefab == null || mainCamera == null)
        {
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (TryGetTileFromRay(ray, out Tile tile))
        {
            TrySpawnOnTile(tile);
        }
    }

    private void TrySpawnFromScreenCenter()
    {
        if (bombPrefab == null || mainCamera == null)
        {
            return;
        }

        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        Ray ray = mainCamera.ScreenPointToRay(screenCenter);

        if (TryGetTileFromRay(ray, out Tile tile))
        {
            TrySpawnOnTile(tile);
        }
    }

    private bool TryGetTileFromRay(Ray ray, out Tile tile)
    {
        tile = null;

        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, tileLayerMask))
        {
            tile = hit.collider.GetComponentInParent<Tile>();
            return tile != null;
        }

        Tile[] tiles = FindObjectsByType<Tile>(FindObjectsSortMode.None);
        if (tiles == null || tiles.Length == 0)
        {
            return false;
        }

        float bestDistance = float.MaxValue;
        Tile bestTile = null;

        foreach (Tile candidate in tiles)
        {
            Vector3 toTile = candidate.transform.position - ray.origin;
            float t = Vector3.Dot(toTile, ray.direction);
            if (t < 0f)
            {
                continue;
            }

            Vector3 closestPoint = ray.origin + ray.direction * t;
            float distance = Vector3.Distance(candidate.transform.position, closestPoint);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestTile = candidate;
            }
        }

        tile = bestTile;
        return tile != null;
    }

    private void TrySpawnOnTile(Tile tile)
    {
        if (tile == null || tile.isObstacle)
        {
            return;
        }

        Vector2Int gridCoordinates = tile.TileCoordinates;
        if (Bomb.IsBombAt(gridCoordinates))
        {
            return;
        }

        Vector3 worldPosition = tile.transform.position + Vector3.up * spawnYOffset;
        Instantiate(bombPrefab, worldPosition, Quaternion.identity);
    }
}
