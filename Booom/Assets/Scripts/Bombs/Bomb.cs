using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Bomb : MonoBehaviour
{
    protected static readonly HashSet<Vector2Int> ActiveBombs = new HashSet<Vector2Int>();

    [SerializeField]
    protected float timer = 3.0f;

    [SerializeField]
    protected float pulseAmplitude = 0.2f;

    [SerializeField]
    protected float pulseSpeed = 8f;

    protected GridManagerStategy _gridManager;
    protected Vector2Int _bombCoordinates;
    protected Vector3 _initialScale;

    public PlayerEnum associatedPlayer = PlayerEnum.None;

    private void Awake()
    {
        GetManagers();
        
        _initialScale = transform.localScale;
        _bombCoordinates = GridManagerStategy.WorldToGridCoordinates(transform.position);
        ActiveBombs.Add(_bombCoordinates);
    }

    protected virtual void Start()
    {
        Fuse();
    }

    public static bool IsBombAt(Vector2Int gridCoordinates)
    {
        return ActiveBombs.Contains(gridCoordinates);
    }

    protected void Fuse()
    {
        StartCoroutine(CountdownAndExplode());
    }

    private IEnumerator CountdownAndExplode()
    {
        float elapsed = 0f;

        while (elapsed < timer)
        {
            float pulse = 1f + (Mathf.Abs(Mathf.Sin(elapsed * pulseSpeed)) * pulseAmplitude);
            transform.localScale = _initialScale * pulse;
            elapsed += Time.deltaTime;
            yield return null;
        }

        Explode();
    }

    protected abstract void Explode();

    protected virtual void OnDestroy()
    {
        ActiveBombs.Remove(_bombCoordinates);
    }

    private void GetManagers()
    {
        _gridManager = FindFirstObjectByType<GridManagerStategy>();

        if (_gridManager == null)
        {
            throw new Exception("There's no active grid manager");
        }
    }
}

public enum BombEnum
{
    None = 0,
    NormalBomb = 1,
    SquareBomb = 2
}