public class HitState : State
{
    private float _hitDuration = GameConstants.HIT_STATE_DURATION;
    public HitState(StateMachine stateMachine, Player player) : base(stateMachine, player)
    { 
    }

    public override void Enter()
    {
        _player.Animator.SetBool("IsHit", true);
        _player.DisableInputActions();
    }

    public override void Exit()
    {
        _player.Animator.SetBool("IsHit", false);
        _hitDuration = GameConstants.HIT_STATE_DURATION;
        _player.EnableInputActions();
    }

    public override void Handle(float time)
    {
        if (_hitDuration <= 0) 
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
        else
        {
            _player.UpdateKnockback();
            _hitDuration -= time;
        }
    }
}
