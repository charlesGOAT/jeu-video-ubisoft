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

        Task timerTask = ManageActiveTime();
        Task movementTask = MoveBombLoop();

        await timerTask;
        await movementTask;
    }

    private async Task MoveBombLoop()
    {
        while (!_cts.IsCancellationRequested)
        {
            ExecuteMove();

            try {
                await Task.Delay(200, _cts.Token); 
            } catch (OperationCanceledException) { break; }
        }
    }

    public void ExecuteMove()
    {
        Vector2Int myGridPos = GridManagerStrategy.WorldToGridCoordinates(_bomb.transform.position);

        Vector2Int targetGridPos = FindClosestEnemy();

        if (targetGridPos != new Vector2Int(-1, -1))
        {
            Vector2Int nextStep = GetNextMoveTowards(myGridPos, targetGridPos);

            _bomb.transform.position = GridManagerStrategy.GridToWorldPosition(nextStep);
            ExplodeIfPlayerInSurroundings(nextStep);
        }
    }
    private async Task ManageActiveTime()
    {
        try
        {
            await Task.Delay((int)(_bomb.Timer * 1000), _cts.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        
        _cts.Cancel();
        _bomb.SetBombCoordinates(GridManagerStrategy.WorldToGridCoordinates(_bomb.transform.position));
        _bomb.Explode();
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
    
    private bool TryGetFreePosInPlayerSurroundings(Vector2Int playerPos, out Vector2Int surroundingPos)
    {
        surroundingPos = Vector2Int.zero;
        
        var availablePos = from pos in _directions
            let tile = GameManager.Instance.GridManager.GetTileAtCoordinates(playerPos + pos)
            where tile != null && !tile.IsObstacle  // todo : and ya pas de bombe
            select pos;

        var availablePosArray = availablePos as Vector2Int[] ?? availablePos.ToArray();

        if (availablePosArray.Length == 0) return false;

        System.Random rand = new System.Random();
        surroundingPos = availablePosArray[rand.Next(0, availablePosArray.Length)];
        return true;
    }
    
    private Vector2Int FindClosestEnemy()
    {
        float minDistance = float.MaxValue;
        Vector2Int targetWorldGridPos = new(-1, -1);
    
        Vector2Int gridBombPos = GridManagerStrategy.WorldToGridCoordinates(_bomb.transform.position);
    
        foreach (Player player in Player.ActivePlayers)
        {
            if (_bomb.AssociatedPlayer == player.PlayerNb) continue;

            Vector2Int playerPos = GridManagerStrategy.WorldToGridCoordinates(player.transform.position);

            if (!TryGetFreePosInPlayerSurroundings(playerPos, out Vector2Int offset)) continue;
        
            Vector2Int actualTargetPos = playerPos + offset;

            float dist = Vector2Int.Distance(gridBombPos, playerPos);
            if (dist < minDistance)
            {
                minDistance = dist;
                targetWorldGridPos = actualTargetPos;
            }
        }

        return targetWorldGridPos;
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

// todo : exploser si elle rencontre un joueur
