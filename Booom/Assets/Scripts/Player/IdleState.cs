using UnityEngine;

public class IdleState : State
{
    public IdleState(StateMachine stateMachine, Player player) : base(stateMachine, player)
    {
    }

    public override void Enter()
    {
        //Jouer l'animation d'idle
        //Pour l'instant on ne fait rien
    }

    public override void Exit()
    {
        //Arrêter l'animation d'idle
    }

    public override void Handle(float time)
    {
        _player.UpdateMovement();

        if (_player.IsMoving())
        {
            _stateMachine.Trigger(GameConstants.PLAYER_RUN_TRIGGER);
        }
    }
}
