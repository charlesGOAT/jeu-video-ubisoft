public class RunState : State
{
    public RunState(StateMachine stateMachine, Player player) : base(stateMachine, player)
    {
    }
    public override void Enter()
    {
        _player.Animator.SetBool("IsRunning", true);
    }
    public override void Exit()
    {
        _player.Animator.SetBool("IsRunning", true);
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

