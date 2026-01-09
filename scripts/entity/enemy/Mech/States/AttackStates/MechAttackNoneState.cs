using Godot;

public class MechAttackIdleState : IState
{
    private readonly Mech _owner;

    public MechAttackIdleState(Mech owner)
    {
        _owner = owner;
    }
    public void Enter() { }

    public void Exit() { }
}
