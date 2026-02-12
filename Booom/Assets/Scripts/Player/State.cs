public abstract class State
{
    protected readonly StateMachine _stateMachine;
    protected readonly Player _player;

    public State(StateMachine stateMachine, Player player)
    {
        _stateMachine = stateMachine;
        _player = player;
    }

    public abstract void Enter();
    public abstract void Handle(float time);
    public abstract void Exit();
}
