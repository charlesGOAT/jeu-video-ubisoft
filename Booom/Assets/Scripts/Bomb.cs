using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [SerializeField]
    private float fuseSeconds = 3f;

    private GridManager _gridManager;
    private Vector2Int _cell;
    private bool _released;

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
        ReleaseCell();
        Destroy(gameObject);
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
}
