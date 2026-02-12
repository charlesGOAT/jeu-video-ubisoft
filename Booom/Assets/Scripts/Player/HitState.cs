using System.Collections;
using UnityEngine;

public class HitState : State
{
    private float _hitDuration = GameConstants.HIT_STATE_DURATION;
    public HitState(StateMachine stateMachine, Player player) : base(stateMachine, player)
    { 
    }

    public override void Enter()
    {
        //Jouer l'animation de hit
    }

    public override void Exit()
    {
        //Arrêter l'animation de hit
        _hitDuration = GameConstants.HIT_STATE_DURATION;
    }

    public override void Handle(float time)
    {
        if (_hitDuration <= 0) 
        {
            _stateMachine.Trigger(GameConstants.PLAYER_IDLE_TRIGGER);
        }
        else
        {
            _hitDuration -= time;
        }
    }
}
