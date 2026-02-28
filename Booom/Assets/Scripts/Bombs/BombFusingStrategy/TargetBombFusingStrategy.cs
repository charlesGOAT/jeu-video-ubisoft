using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class TargetBombFusingStrategy : BombFusingStrategy
{
    private Vector2Int[] _directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
    
    private CancellationTokenSource _cts;

    private Bomb _bomb;

    public override async void Fuse(Bomb bomb)
    {
        _bomb = bomb;
        _cts = new CancellationTokenSource();
        
        _bomb.StartPulseCoroutine();

        Task timerTask = ManageActiveTime(); // bomb explodes after the Timer is over even if it hasn't reached any player
        Task movementTask = MoveBombLoop();

        await timerTask;
        await movementTask;
    }
    
    private async Task ManageActiveTime()
    {
        try
        {
            await Task.Delay((int)(_bomb.Timer * 1000), _cts.Token);
        }
        catch (OperationCanceledException) { return; }
        
        _cts.Cancel();
        _bomb.SetBombCoordinates(GridManagerStrategy.WorldToGridCoordinates(_bomb.transform.position));
        _bomb.Explode();
    }

    private async Task MoveBombLoop()
    {
        float moveSpeed = 6f; // Tiles per second

        while (!_cts.IsCancellationRequested)
        {
            Vector2Int myGridPos = GridManagerStrategy.WorldToGridCoordinates(_bomb.transform.position);
            Vector2Int targetGridPos = FindClosestEnemy();

            if (targetGridPos != new Vector2Int(-1, -1))
            {
                Vector2Int nextGridStep = GetNextMoveTowards(myGridPos, targetGridPos);
                Vector3 targetWorldPos = GridManagerStrategy.GridToWorldPosition(nextGridStep);

                while (!_cts.IsCancellationRequested && 
                       Vector3.Distance(_bomb.transform.position, targetWorldPos) > 0.01f)
                {
                    _bomb.transform.position = Vector3.MoveTowards(
                        _bomb.transform.position, 
                        targetWorldPos, 
                        moveSpeed * Time.deltaTime
                    );
                
                    await Task.Yield(); 
                }
            
                _bomb.SetBombCoordinates(nextGridStep);
                ExplodeIfPlayerInSurroundings(nextGridStep);
            }
            else
            {
                try
                {
                    await Task.Delay(100, _cts.Token);
                }
                catch (OperationCanceledException) { break; }
            }
        }
    }

    private Vector2Int FindClosestEnemy()
    {
        float minDistance = float.MaxValue;
        Vector2Int targetGridPos = new(-1, -1);
    
        Vector2Int gridBombPos = GridManagerStrategy.WorldToGridCoordinates(_bomb.transform.position);
    
        foreach (Player player in Player.ActivePlayers)
        {
            if (_bomb.AssociatedPlayer == player.PlayerNb) continue;

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
            if (_bomb.AssociatedPlayer == player.PlayerNb) continue;

            Vector2Int playerPos = GridManagerStrategy.WorldToGridCoordinates(player.transform.position);
            if (Vector2Int.Distance(newPos, playerPos) < _bomb.ExplosionRange)
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
        var tile = GameManager.Instance.GridManager.GetTileAtCoordinates(coords);
        return tile != null && !tile.IsObstacle;
    }

    public override void OnCollision(Bomb bomb)
    {
        _cts.Cancel();
        bomb.SetBombCoordinates(GridManagerStrategy.WorldToGridCoordinates(bomb.transform.position));
        bomb.Explode();
    }
}