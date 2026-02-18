using UnityEngine;

public class JumpState : State
{
    private float _jumpDuration = GameConstants.AIR_STATE_DURATION;
    public JumpState(StateMachine stateMachine, Player player) : base(stateMachine, player)
    {
    }

    public override void Enter()
    {
        //Jouer l'animation de hit
        _jumpDuration = GameConstants.AIR_STATE_DURATION;
        _player.DisableInputActions();
    }

    public override void Exit()
    {
        //Arrêter l'animation de hit
        _player.EnableInputActions();
    }

    public override void Handle(float time)
    {
        //Check si y'est groundé aussi
        if (_jumpDuration <= 0)
        {
            _stateMachine.Trigger(GameConstants.PLAYER_IDLE_TRIGGER);
        }
        else
        {
            _jumpDuration -= time;
        }
    }
}

