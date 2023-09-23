using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class PlayerState
{
    internal bool _isRootState = false;
    internal PlayerStateMachine _ctx;
    internal PlayerStateFactory _factory;
    internal PlayerState _currentSuperState;
    internal PlayerState _currentSubState;

    public PlayerStateType StateIdentifier = PlayerStateType.None;
    protected bool IsRootState { get { return _isRootState; } set { _isRootState = value; } }
    protected PlayerStateMachine Context { get { return _ctx; } }
    protected PlayerState CurrentSuperState { get { return _currentSuperState; } }
    protected PlayerState CurrentSubState { get { return _currentSubState; } }
    protected PlayerStateFactory Factory { get { return _factory; } }
    public PlayerState(PlayerStateMachine ctx, PlayerStateFactory factory, PlayerStateType identifier)
    {
        _ctx = ctx;
        _factory = factory;
        StateIdentifier = identifier;
    }
    public string StateName;
    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
    public abstract void CheckSwitchStates();
    public abstract void InitializeSubState();
    public abstract void OnCollisionEnter();
    public abstract void OnCollisionStay();
    public abstract void OnCollisionExit();

    public void UpdateStates()
    {
        UpdateState();
        _currentSubState?.UpdateState();
    }

    public void ExitStates()
    {
        ExitState();
        _currentSubState?.ExitState();
    }
    protected void SwitchState(PlayerState nextState)
    {
        ExitStates();
        nextState.EnterState();
        //Debug.Log("Enter state call on SwitchState");
        if (_isRootState)
        {
            _ctx.CurrentState = nextState;
        }
        else if (_currentSuperState != null)
        {
            _currentSuperState.SetSubState(nextState);
        }
    }
    protected void SetSuperState(PlayerState newSuperState)
    {
        _currentSuperState = newSuperState;
    }
    protected void SetSubState(PlayerState newSubState)
    {

        _currentSubState = newSubState;
        //_currentSubState.EnterState();
        //Debug.Log("Enter state call on SetSubState");
        newSubState.SetSuperState(this);
    }
}