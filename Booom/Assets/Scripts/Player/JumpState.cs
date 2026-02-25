
public class JumpState : State
{
    private float _airDuration = GameConstants.AIR_STATE_DURATION;

    public JumpState(StateMachine stateMachine, Player player) : base(stateMachine, player)
    {
    }


    public override void Enter()
    {
        _airDuration = GameConstants.AIR_STATE_DURATION;
    }

    public override void Exit()
    {
        _player.ResetJumpVelocity();
    }

    public override void Handle(float time)
    {
        _player.UpdateJump();
        _airDuration -= time;

        if (_airDuration <= 0f || _player.GetIsGrounded())
        {
            if (_player.IsMoving())
            {
                _stateMachine.Trigger(GameConstants.PLAYER_RUN_TRIGGER);
            }
            else
            {
                _stateMachine.Trigger(GameConstants.PLAYER_IDLE_TRIGGER);
            }
        }
    }


}
