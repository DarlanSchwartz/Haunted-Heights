
using System.Collections.Generic;
public enum PlayerStateType 
{
    None,
    Grounded,
    Idle, 
    Running, 
    Jumping,
    Walking,
    Balancing, 
    Climbing,
    Crouched,
    Falling,
    Landing,
    Proning, 
    Sliding,
    Vaulting,
    Squeezing,
    JumpingOnto,
    OpeningDoor,
    OpeningWindow,
    InLadder,
};

public class PlayerStateFactory
{
    PlayerStateMachine _context;
    Dictionary<PlayerStateType, PlayerState> _states = new Dictionary<PlayerStateType, PlayerState>();
    public PlayerStateFactory(PlayerStateMachine currentContext)
    {
        _context = currentContext;
        _states[PlayerStateType.Grounded] = new PlayerStateGrounded(_context, this);//OK
        _states[PlayerStateType.Idle]  = new PlayerStateIdle(_context, this);//OK
        _states[PlayerStateType.Walking]  = new PlayerStateWalk(_context, this);//OK
        _states[PlayerStateType.Running]  = new PlayerStateRun(_context, this);//OK
        _states[PlayerStateType.Jumping]  = new PlayerStateJump(_context, this);// OK'ish, need to fix forces
        _states[PlayerStateType.Falling]  = new PlayerStateFalling(_context, this);//OK
        _states[PlayerStateType.Landing]  = new PlayerStateLand(_context, this);//OK
        _states[PlayerStateType.Sliding]  = new PlayerStateSlide(_context, this);//OK
        _states[PlayerStateType.Vaulting]  = new PlayerStateVault(_context, this);//OK
        _states[PlayerStateType.Crouched]  = new PlayerStateCrouched(_context, this);//OK
        _states[PlayerStateType.Proning]  = new PlayerStateProne(_context, this);//OK
        _states[PlayerStateType.JumpingOnto] = new PlayerStateJumpOnto(_context, this);//ok
        _states[PlayerStateType.Climbing] = new PlayerStateClimb(_context, this);//OK -> Need to polish final hang anim

        _states[PlayerStateType.InLadder] = new PlayerStateClimb(_context, this);
        _states[PlayerStateType.Squeezing]  = new PlayerStateIdle(_context, this);
        _states[PlayerStateType.Balancing]  = new PlayerStateIdle(_context, this);
    }

    public PlayerState Idle() => _states[PlayerStateType.Idle];
    public PlayerState Walk() => _states[PlayerStateType.Walking];
    public PlayerState Run() => _states[PlayerStateType.Running];
    public PlayerState Jump() => _states[PlayerStateType.Jumping];
    public PlayerState Grounded() => _states[PlayerStateType.Grounded];
    public PlayerState Land() => _states[PlayerStateType.Landing];
    public PlayerState Slide() => _states[PlayerStateType.Sliding];
    public PlayerState Vault() => _states[PlayerStateType.Vaulting];
    public PlayerState Climb()=> _states[PlayerStateType.Climbing];
    public PlayerState Crouched()=> _states[PlayerStateType.Crouched];
    public PlayerState Falling()=> _states[PlayerStateType.Falling];
    public PlayerState Prone()=> _states[PlayerStateType.Proning];
    public PlayerState JumpOnto()=> _states[PlayerStateType.JumpingOnto];

}