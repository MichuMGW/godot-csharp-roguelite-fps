using Godot;

public class PlayerMoveGroundedState : IState, IPhysicsUpdateState
{
    private readonly Player _player;

    public PlayerMoveGroundedState(Player player)
    {
        _player = player;
    }

    public void Enter()
    {
        _player.Movement.ResetJumps();
    }

    public void Exit() { }

    public void PhysicsUpdate(double delta)
    {
        float dt = (float)delta;

        if (!_player.IsOnFloor())
        {
            _player.ChangeMoveState(PlayerMoveStateId.Airborne);
            return;
        }

        Vector3 moveDir = _player.Movement.ReadMoveInput();
        float speed = _player.Movement.Speed;

        _player.Movement.ApplyGroundMove(moveDir, speed);

        // Jump
        if (_player.Movement.TryJump())
        {
            _player.ChangeMoveState(PlayerMoveStateId.Airborne);
            return;
        }

        _player.Movement.ApplyGravity(dt);
    }
}
