using System.Collections.Generic;
using UnityEngine;

public static class GameConstants
{
    public const short NB_PLAYERS = 4;
    public const float COLOR_BOOST = 1.5f;
    public const float COLOR_DEBUFF = 0.5f;
    public const string PLAYER_IDLE_TRIGGER = "Idle";
    public const string PLAYER_RUN_TRIGGER = "Run";
    public const string PLAYER_HIT_TRIGGER = "Hit";
    public const string PLAYER_JUMP_TRIGGER = "Jump";
    public const string PLAYER_BOMB_TRIGGER = "DropBomb";
    public const int UNITY_GRID_SIZE = 2;
    public const float HIT_STATE_DURATION = 1.5f;
    public const float GAME_DURATION = 120f;
    public const float AIR_STATE_DURATION = 1.0f;
    public const float POPUP_DURATION = 2.0f;

    public static readonly Dictionary<int, float> SpeedBoostPerKill =new ()
    {
        {0, 1f},
        {1, 1.25f},
        {2, 1.5f},
        {3, 1.75f},
        {4, 2f},
        {5, 2.25f} // todo : add more or tweak
    };
    
    public static readonly Dictionary<int, int> RangeBoostPerKill = new ()
    {
        {0, 0},
        {2, 1},
        {4, 2},
        {6, 3}  // todo : add more or tweak
    };
}
