using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    public LadderPoint startPoint;
    public LadderPoint endPoint;
    public Transform endClimbReference;
    public Transform LeftHandIK;
    public Transform RightHandIK;
    public Transform LeftFootIK;
    public Transform RightFootIK;
    public Transform playerReferencePosition;
    public List<LadderPoint> points;
    public float TargetDelta { get; private set; }



    public void SetTargetDelta(LadderPoint lp)
    {
        TargetDelta = lp.deltaTarget;
    }

    public void SetTargetDelta(float target)
    {
        TargetDelta = target;
    }

    [SerializeField] private Animator animator;
    private readonly int DeltaHash = Animator.StringToHash("Delta");
    private readonly int ClimbHash = Animator.StringToHash("Climb");
    private readonly int ResetHash = Animator.StringToHash("Reset");
    public bool UpdateDelta
    {
        get
        {
            return m_UpdateDelta;
        }
        set
        {
            m_UpdateDelta = value;
        }
    }

    private bool m_UpdateDelta = false;


    private void Update()
    {
        if(!m_UpdateDelta)
        {
            return;
        }

        animator.SetFloat(DeltaHash, Mathf.LerpUnclamped(CurrentDelta, TargetDelta, 5 * Time.deltaTime));
    }


    private void Awake()
    {
        if(!animator)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private float CurrentDelta
    {
        get
        {
            return animator.GetFloat(DeltaHash);
        }
    }

    public void ResetDelta(bool setResetTrigger)
    {
        animator.SetFloat(DeltaHash,0);
        TargetDelta = 0;

        if(setResetTrigger)
        {
            animator.SetTrigger(ResetHash);
        }
    }

    public void Climb()
    {
        animator.SetTrigger(ClimbHash);
    }
}

