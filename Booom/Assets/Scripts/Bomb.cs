using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [SerializeField]
    private float fuseSeconds = 3f;
    [SerializeField]
    private int explosionRangeCells = 3; // cells outward in each cardinal direction
    [SerializeField]
    private float explosionDuration = 0.25f;
    [SerializeField]
    private Color explosionColor = new Color(1f, 0.6f, 0.15f, 1f);

    private GridManager _gridManager;
    private Vector2Int _cell;
    private bool _released;
    private bool _detonated;

    public Vector2Int Cell => _cell;

    public void Init(GridManager gridManager, Vector2Int cell, float fuse)
    {
        _gridManager = gridManager;
        _cell = cell;
        fuseSeconds = fuse;
    }

    private void Start()
    {
        StartCoroutine(FuseRoutine());
    }

    private IEnumerator FuseRoutine()
    {
        yield return new WaitForSeconds(fuseSeconds);
        TriggerDetonate();
    }

    private void OnDestroy()
    {
        ReleaseCell();
    }

    private void ReleaseCell()
    {
        if (_released) return;
        _released = true;

        if (_gridManager != null)
        {
            _gridManager.ReleaseCell(_cell);
        }
    }

    public void TriggerDetonate()
    {
        if (_detonated) return;
        _detonated = true;

        // Grid-based cross explosion
        ExplodeGridCross();

        // Ensure the grid cell is freed
        ReleaseCell();

        // Remove this bomb
        Destroy(gameObject);
    }

    private void ExplodeGridCross()
    {
        if (_gridManager == null) return;

        // Center
        SpawnExplosionSegment(_cell);

        // Four directions
        Vector2Int[] dirs = new[]
        {
            new Vector2Int(1, 0),  // +X
            new Vector2Int(-1, 0), // -X
            new Vector2Int(0, 1),  // +Z
            new Vector2Int(0, -1)  // -Z
        };

        foreach (var dir in dirs)
        {
            for (int i = 1; i <= explosionRangeCells; i++)
            {
                Vector2Int c = new Vector2Int(_cell.x + dir.x * i, _cell.y + dir.y * i);
                if (!_gridManager.IsInBounds(c)) break;
                SpawnExplosionSegment(c);
            }
        }

        // Chain reaction: detonate bombs in affected cells
        Bomb[] bombs = FindObjectsOfType<Bomb>();
        foreach (var b in bombs)
        {
            if (b == null || b == this) continue;
            if (IsCellInCross(b.Cell))
            {
                b.TriggerDetonate();
            }
        }
    }

    private bool IsCellInCross(Vector2Int other)
    {
        if (other == _cell) return true;
        if (other.y == _cell.y)
        {
            int dx = Mathf.Abs(other.x - _cell.x);
            return dx > 0 && dx <= explosionRangeCells;
        }
        if (other.x == _cell.x)
        {
            int dz = Mathf.Abs(other.y - _cell.y);
            return dz > 0 && dz <= explosionRangeCells;
        }
        return false;
    }

    private void SpawnExplosionSegment(Vector2Int cell)
    {
        Vector3 pos = _gridManager.GetCellCenter(cell);
        pos.y = transform.position.y; // align with bomb height

        // Visual quad on ground
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "ExplosionSegment";
        quad.transform.position = pos + Vector3.up * 0.01f; // slight lift to avoid z-fighting
        quad.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // lay on XZ plane
        float s = _gridManager.TileSize * 0.85f;
        quad.transform.localScale = new Vector3(s, s, 1f);
        var renderer = quad.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = explosionColor;
        }
        var collider = quad.GetComponent<Collider>();
        if (collider != null) collider.enabled = false;

        // Optional: add a simple pulsing effect
        var fx = quad.AddComponent<ExplosionFX>();
        fx.Duration = explosionDuration;
        fx.PulseScale = 1.15f;

        // Paint the tile temporarily for "splatoon" effect
        _gridManager.PaintCell(cell, explosionColor, explosionDuration);

        Destroy(quad, explosionDuration);
    }
}
