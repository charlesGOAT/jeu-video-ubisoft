using UnityEngine;

public class RunState : State
{
    public RunState(StateMachine stateMachine, Player player) : base(stateMachine, player)
    {
    }
    public override void Enter()
    {
        //Jouer l'animation de course
        //Pour l'instant on ne fait rien
    }
    public override void Exit()
    {
        //Arrêter l'animation de course
    }
    public override void Handle(float time)
    {
        _player.UpdateMovement();
        
        if (!_player.IsMoving()) 
        {
            _stateMachine.Trigger(GameConstants.PLAYER_IDLE_TRIGGER);
        }
    }
}

