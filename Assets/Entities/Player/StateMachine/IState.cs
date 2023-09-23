using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IState
{
    public abstract string Name { get; }

    protected abstract void OnStart();
    protected abstract void OnUpdate();
    protected abstract void OnExit();
}
