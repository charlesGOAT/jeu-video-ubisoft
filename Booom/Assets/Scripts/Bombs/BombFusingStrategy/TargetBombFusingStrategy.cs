using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class TargetBombFusingStrategy : BombFusingStrategy
{
    private Vector2Int[] _directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
    
    private CancellationTokenSource _cts;

    private Player _associatedPlayer; 
    private Bomb _bomb;

    private readonly System.Object _lock = new ();

    public override async void Fuse(Bomb bomb)
    {
        _bomb = bomb;
        _associatedPlayer = Player.ActivePlayers.First(player => player.PlayerNb == _bomb.AssociatedPlayer);
        _cts = new CancellationTokenSource();

        _bomb.StartPulseCoroutine();

        Awaitable timerTask = ManageActiveTime(); // bomb explodes after the Timer is over even if it hasn't reached any player
        Awaitable movementTask = MoveBombLoop();

        await timerTask;
        await movementTask;
    }
    
    private async Awaitable ManageActiveTime()
    {
        try
        {
            await Awaitable.WaitForSecondsAsync(_bomb.Timer, _cts.Token);
        }
        catch (OperationCanceledException) { return; }
        
        _cts.Cancel();

        lock (_lock)
        {
            if (_bomb == null) return;
            _bomb.SetBombCoordinates(GridManagerStrategy.WorldToGridCoordinates(_bomb.transform.position));
            _bomb.Explode();
        }
    }

    private async Awaitable MoveBombLoop()
    {
        float moveSpeed = 8f;

        while (!_cts.IsCancellationRequested)
        {
            Vector2Int myGridPos = new();
            lock (_lock)
            {
                if (_bomb == null) break;
                myGridPos = GridManagerStrategy.WorldToGridCoordinates(_bomb.transform.position);
            }

            Vector2Int targetGridPos = FindClosestEnemy();

            if (targetGridPos != new Vector2Int(-1, -1))
            {
                Vector2Int nextGridStep = GetNextMoveTowards(myGridPos, targetGridPos);
                Vector3 targetWorldPos = GridManagerStrategy.GridToWorldPosition(nextGridStep);

                while (!_cts.IsCancellationRequested)
                {
                    lock (_lock)
                    {
                        if (_bomb == null || Vector3.Distance(_bomb.transform.position, targetWorldPos) <= 0.01f) break;
                    
                        _bomb.transform.position = Vector3.MoveTowards(
                            _bomb.transform.position, 
                            targetWorldPos, 
                            moveSpeed * Time.deltaTime
                        );
                    }
                    
                    await Awaitable.NextFrameAsync(); 
                }

                lock (_lock)
                {
                    if (_bomb == null) break;
                    _bomb.SetBombCoordinates(nextGridStep);
                }
                
                ExplodeIfPlayerInSurroundings(nextGridStep);
            }
            else
            {
                try
                {
                    await Awaitable.WaitForSecondsAsync(0.100f, _cts.Token);
                }
                catch (OperationCanceledException) { break; }
            }
        }
    }

    private Vector2Int FindClosestEnemy()
    {
        float minDistance = float.MaxValue;
        Vector2Int targetGridPos = new(-1, -1);
        Vector2Int gridBombPos = new();
        
        lock (_lock)
        {
            if (_bomb == null) return targetGridPos;
            gridBombPos = GridManagerStrategy.WorldToGridCoordinates(_bomb.transform.position);
        }
    
        foreach (Player player in Player.ActivePlayers)
        {
            if (_associatedPlayer.PlayerNb == player.PlayerNb) continue;

            Vector2Int playerPos = GridManagerStrategy.WorldToGridCoordinates(player.transform.position);

            if (!TryGetFreePosInPlayerSurroundings(playerPos, gridBombPos, out Vector2Int actualTargetPos)) continue;
        
            float dist = Vector2Int.Distance(gridBombPos, playerPos);
            if (dist < minDistance)
            {
                minDistance = dist;
                targetGridPos = actualTargetPos;
            }
        }

        return targetGridPos;
    }

    private bool TryGetFreePosInPlayerSurroundings(Vector2Int playerPos, Vector2Int bombPos, out Vector2Int surroundingPos)
    {
        surroundingPos = Vector2Int.zero;

        var availablePos = from pos in _directions
            let realPos = playerPos + pos
            let tile = GameManager.Instance.GridManager.GetTileAtCoordinates(realPos)
            where tile != null && !tile.IsObstacle && !Bomb.IsBombAt(realPos)
            select realPos;

        var availablePosArray = availablePos as Vector2Int[] ?? availablePos.ToArray();

        if (availablePosArray.Length == 0) return false;

        surroundingPos = GetClosestPosToBomb(availablePosArray, bombPos);
        return true;
    }

    private Vector2Int GetClosestPosToBomb(in Vector2Int[] surroundingPos, Vector2Int bombPos)
    {
        return surroundingPos.OrderBy(pos => Vector2Int.Distance(pos, bombPos)).First();
    }
    
    private void ExplodeIfPlayerInSurroundings(Vector2Int newPos)
    {
        foreach (Player player in Player.ActivePlayers)
        {
            if (_associatedPlayer.PlayerNb == player.PlayerNb) continue;

            Vector2Int playerPos = GridManagerStrategy.WorldToGridCoordinates(player.transform.position);

            bool shouldExplode = false;
            lock (_lock)
            {
                if (_bomb == null) return;
                shouldExplode = Vector2Int.Distance(newPos, playerPos) < _bomb.ExplosionRange;
            }

            if (shouldExplode)
            {
                OnCollision(_bomb);
            }
        }
    }
    
    private Vector2Int GetNextMoveTowards(Vector2Int startPos, Vector2Int targetPos)
    {
        if (startPos == targetPos) return startPos;

        Queue<(Vector2Int current, Vector2Int firstStep)> queue = new();
        HashSet<Vector2Int> visited = new();

        foreach (var dir in _directions)
        {
            Vector2Int neighbor = startPos + dir;
            if (IsTileWalkable(neighbor))
            {
                queue.Enqueue((neighbor, dir));
                visited.Add(neighbor);
            }
        }

        while (queue.Count > 0)
        {
            var (curr, firstStep) = queue.Dequeue();

            if (curr == targetPos) return startPos + firstStep;

            foreach (var dir in _directions)
            {
                Vector2Int next = curr + dir;
                if (IsTileWalkable(next) && !visited.Contains(next))
                {
                    visited.Add(next);
                    queue.Enqueue((next, firstStep));
                }
            }
        }

        return startPos; // No path found, stay put
    }
    
    private bool IsTileWalkable(Vector2Int coords)
    {
        Vector2Int playerPos = GridManagerStrategy.WorldToGridCoordinates(_associatedPlayer.transform.position);

        var tile = GameManager.Instance.GridManager.GetTileAtCoordinates(coords);
        return tile != null && !tile.IsObstacle && coords != playerPos;
    }

    public override void OnCollision(Bomb bomb)
    {
        _cts.Cancel();
        lock (_lock)
        {
            if (_bomb == null) return;
            bomb.SetBombCoordinates(GridManagerStrategy.WorldToGridCoordinates(bomb.transform.position));
            bomb.Explode();
        }
    }
}