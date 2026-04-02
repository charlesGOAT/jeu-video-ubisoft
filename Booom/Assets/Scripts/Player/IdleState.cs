using UnityEngine;

public delegate bool OnBombPerformed();
public class IdleState : State
{
    public IdleState(StateMachine stateMachine, Player player) : base(stateMachine, player)
    {
    }

    public override void Enter()
    {
        _player.Animator.SetBool("IsIdle", true);
    }

    public override void Exit()
    {
        _player.Animator.SetBool("IsIdle", true);
    }

    public override void Handle(float time)
    {
        //Peut causer des issues? a revoir
        _player.UpdateMovement();

        if (_player.IsMoving())
        {
            _stateMachine.Trigger(GameConstants.PLAYER_RUN_TRIGGER);
        }
    }
}
