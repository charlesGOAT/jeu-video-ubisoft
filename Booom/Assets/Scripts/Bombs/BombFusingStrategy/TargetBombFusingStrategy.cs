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
    
    private const float MOVE_SPEED = 8f;


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
        while (!_cts.IsCancellationRequested)
        {
            Vector2Int bombGridPos = new();
            lock (_lock)
            {
                if (_bomb == null) break;
                bombGridPos = GridManagerStrategy.WorldToGridCoordinates(_bomb.transform.position);
            }

            Vector2Int nextGridStep = GetNextBombPos(bombGridPos, new());

            if (nextGridStep != bombGridPos)
            {
                while (!_cts.IsCancellationRequested)
                {
                    lock (_lock)
                    {
                        if (_bomb == null || Vector2Int.Distance(bombGridPos, nextGridStep) <= 0.01f) break;
                        MoveBomb(nextGridStep);
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
                if (await RestBeforeRetry()) break;
            }
        }
    }

    private void MoveBomb(in Vector2Int newBombPos)
    {
        Vector3 targetWorldPos = GridManagerStrategy.GridToWorldPosition(newBombPos);

        _bomb.transform.position = Vector3.MoveTowards(
            _bomb.transform.position, 
            targetWorldPos, 
            MOVE_SPEED * Time.deltaTime
        );
    }
    
    private async Awaitable<bool> RestBeforeRetry()
    {
        try
        {
            await Awaitable.WaitForSecondsAsync(0.100f, _cts.Token);
        }
        catch (OperationCanceledException) { return false; }
        return true;
    }

    private Vector2Int GetNextBombPos(in Vector2Int gridBombPos, in List<PlayerEnum> notAvailablePlayers)
    {
        float minDistance = float.MaxValue;
        Vector2Int defaultPos = new Vector2Int(int.MaxValue, int.MaxValue);
        Vector2Int targetGridPos = defaultPos;
        PlayerEnum closestPlayer = PlayerEnum.None;

        GetPlayerMinDistance(gridBombPos, notAvailablePlayers, ref minDistance, ref closestPlayer, ref targetGridPos);

        if (targetGridPos == defaultPos) return gridBombPos;
        
        Vector2Int nextMove = GetNextMoveTowards(gridBombPos, targetGridPos);
        if (nextMove != gridBombPos) return nextMove;
        
        notAvailablePlayers.Add(closestPlayer);
        if (notAvailablePlayers.Count == Player.ActivePlayers.Count) return gridBombPos;
        return GetNextBombPos(gridBombPos, notAvailablePlayers);
    }

    private void GetPlayerMinDistance(in Vector2Int gridBombPos, in List<PlayerEnum> notAvailablePlayers, ref float minDistance,
        ref PlayerEnum closestPlayer, ref Vector2Int targetGridPos)
    {
        foreach (Player player in Player.ActivePlayers)
        {
            if (_associatedPlayer.PlayerNb == player.PlayerNb
                || notAvailablePlayers.Contains(player.PlayerNb)) continue;

            Vector2Int playerPos = GridManagerStrategy.WorldToGridCoordinates(player.transform.position);

            if (!TryGetFreePosInPlayerSurroundings(playerPos, gridBombPos, out Vector2Int actualTargetPos))
                continue;

            float dist = Vector2Int.Distance(gridBombPos, playerPos);
            if (dist < minDistance)
            {
                closestPlayer = player.PlayerNb;
                minDistance = dist;
                targetGridPos = actualTargetPos;
            }
        }
    }

    private bool TryGetFreePosInPlayerSurroundings(Vector2Int playerPos,  in Vector2Int bombPos, out Vector2Int surroundingPos)
    {
        surroundingPos = Vector2Int.zero;

        var availablePos = from pos in _directions
            let realPos = playerPos + pos
            let tile = GameManager.Instance.GridManager.GetTileAtCoordinates(realPos)
            where IsTileWalkable(realPos)
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
    
    private void ExplodeIfPlayerInSurroundings(in Vector2Int newPos)
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
    
    private Vector2Int GetNextMoveTowards(in Vector2Int startPos, in Vector2Int targetPos)
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
    
    private bool IsTileWalkable(in Vector2Int coords)
    {
        Vector2Int playerPos = GridManagerStrategy.WorldToGridCoordinates(_associatedPlayer.transform.position);

        var tile = GameManager.Instance.GridManager.GetTileAtCoordinates(coords);
        return tile != null && !tile.IsObstacle && coords != playerPos;
    }

    public override void OnCollision(in Bomb bomb)
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
