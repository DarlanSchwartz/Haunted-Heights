using UnityEngine;

public class HandIK : MonoBehaviour
{
    public Animator Animator { get; set;}

    public bool PlaceLeft;
    public bool PlaceRight;

    public Transform DefaultLeftHandIK;
    public Transform DefaultRightHandIK;

    private Transform currentLeftTarget;
    private Transform currentRightTarget;

    private float m_leftWeight = 0.0f;
    private float m_rightWeight = 0.0f;

    public float LeftWeight
    {
        get
        {
            return m_leftWeight;
        }
        set
        {
            m_leftWeight = value;
        }
    }

    public float RightWeight
    {
        get
        {
            return m_rightWeight;
        }
        set
        {
            m_rightWeight = value;
        }
    }

    private bool FollowTargets;

    public float WeightSpeed  = 1.0f;
    public float LookAtWeightSpeed = 1.0f;
    public float IKSpeed = 4.0f;
    private float m_defaultWeightSpeed;
    private bool m_manualWeights = false;
    public bool ManualWeights
    {
        get 
        { 
            return m_manualWeights; 
        } 
        set 
        { 
            m_manualWeights = value; 
        }
    }
    public bool lookElbows { get; private set; }
    public bool BodyLook { get; private set; }

    public Vector3 BodyLookAtPosition { get; private set;}

    private void Awake()
    {
        m_defaultWeightSpeed = WeightSpeed;

        SetDefaultIKTargets();
    }

    public void SetDefaultIKTargets()
    {
        SetLeftTarget(DefaultLeftHandIK);
        SetRightTarget(DefaultRightHandIK);
    }

    public void StartFollowingTargets(bool left,bool right)
    {
        FollowTargets = true;
        PlaceLeft = left;
        PlaceRight = right;

        if(!PlaceLeft && !PlaceRight)
        {
            FollowTargets = false;
        }
    }

    public void SetFollowTargets(Transform left, Transform right)
    {
        SetLeftTarget(left);
        SetRightTarget(right);
    }

    public void SetLeftTarget(Transform leftTarget)
    {
        if (leftTarget != null)
        {
            currentLeftTarget = leftTarget;
        }
    }

    public void SetRightTarget(Transform rightTarget)
    {
        if (rightTarget != null)
        {
            currentRightTarget = rightTarget;
        }
    }

    public void StopFollowingTargets()
    {
        FollowTargets = false;
        PlaceLeft = false;
        PlaceRight = false;
    }

    public void SetTargetsParent(Transform parent)
    {
        currentLeftTarget.SetParent(parent);
        currentRightTarget.SetParent(parent);
    }

    public void SetLeftTargetParent(Transform parent)
    {
        currentLeftTarget.SetParent(parent);
    }

    public void SetRightTargetParent(Transform parent)
    {
        currentRightTarget.SetParent(parent);
    }

    public void SetTargetPositions(Vector3 leftPos,Vector3 rightPos,bool lerp)
    {
        if(!lerp)
        {
            currentLeftTarget.position = leftPos;
            currentRightTarget.position = rightPos;
        }
        else
        {
            currentRightTarget.position = Vector3.Lerp(currentRightTarget.position, rightPos, IKSpeed * Time.deltaTime);
            currentLeftTarget.position = Vector3.Lerp(currentLeftTarget.position, leftPos, IKSpeed * Time.deltaTime);
        }
    }

    public void SetTargetPositions(Vector3 leftPos, Vector3 rightPos, bool lerp, float speed)
    {
        if (!lerp)
        {
            currentLeftTarget.position = leftPos;
            currentRightTarget.position = rightPos;
        }
        else
        {
            currentRightTarget.position = Vector3.Lerp(currentRightTarget.position, rightPos, speed * Time.deltaTime);
            currentLeftTarget.position = Vector3.Lerp(currentLeftTarget.position, leftPos, speed * Time.deltaTime);
        }
    }

    public void SetRightTargetPosition(Vector3 target, bool lerp)
    {
        if(!lerp)
        {
            currentRightTarget.position = target;
        }
        else
        {
            currentRightTarget.position = Vector3.Lerp(currentRightTarget.position, target, IKSpeed * Time.deltaTime);
        }
    }

    public void SetLeftTargetPosition(Vector3 target, bool lerp)
    {
        if(!lerp)
        {
            currentLeftTarget.position = target;
        }
        else
        {
            currentLeftTarget.position = Vector3.Lerp(currentLeftTarget.position, target, IKSpeed * Time.deltaTime);
        }
    }

    public void SetTargetRotations(Quaternion leftRot, Quaternion rightRot)
    {
        currentLeftTarget.rotation = leftRot;
        currentRightTarget.rotation = rightRot;
    }

    public void SetTargetRotations(Quaternion leftRot, Quaternion rightRot, bool lerp)
    {
        if (!lerp)
        {
            currentLeftTarget.rotation = leftRot;
            currentRightTarget.rotation = rightRot;
            return;
        }
        else
        {
            currentLeftTarget.rotation = Quaternion.Lerp(currentLeftTarget.rotation, leftRot, IKSpeed * Time.deltaTime);
            currentRightTarget.rotation = Quaternion.Lerp(currentLeftTarget.rotation, rightRot, IKSpeed * Time.deltaTime);
        }
    }

    public void SetTargetRotations(Quaternion leftRot, Quaternion rightRot, bool lerp, float speed)
    {
        if (!lerp)
        {
            currentLeftTarget.rotation = leftRot;
            currentRightTarget.rotation = rightRot;
            return;
        }
        else
        {
            currentLeftTarget.rotation = Quaternion.Lerp(currentLeftTarget.rotation, leftRot, speed * Time.deltaTime);
            currentRightTarget.rotation = Quaternion.Lerp(currentLeftTarget.rotation, rightRot, speed * Time.deltaTime);
        }
    }

    public Vector3 RightTargetPos
    {
        get
        {
            return currentRightTarget.position;
        }
    }

    public Vector3 LeftTargetPos
    {
        get
        {
            return currentRightTarget.position;
        }
    }

    public float LookAtWeight { get; private set; }
    private Transform rightElbowPosition;
    private Transform leftElbowPosition;

    private void OnAnimatorIK(int layerIndex)
    {
        if(!FollowTargets)
        {
            if(m_leftWeight > 0)
            {
                m_leftWeight -= WeightSpeed * Time.deltaTime;
                m_leftWeight = Mathf.Clamp(m_leftWeight, 0, 1);
                Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, m_leftWeight);
                Animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, m_leftWeight);
            }

            if(m_rightWeight > 0)
            {
                m_rightWeight -= WeightSpeed * Time.deltaTime;
                m_rightWeight = Mathf.Clamp(m_rightWeight, 0, 1);
                Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, m_rightWeight);
                Animator.SetIKRotationWeight(AvatarIKGoal.RightHand, m_rightWeight);
            }
            return;
        }
        else
        {
            if(ManualWeights)
            {
                if (PlaceLeft)
                {
                    Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, m_leftWeight);
                    Animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, m_leftWeight);
                    Animator.SetIKPosition(AvatarIKGoal.LeftHand, currentLeftTarget.position);
                    Animator.SetIKRotation(AvatarIKGoal.LeftHand, currentLeftTarget.rotation);
                }

                if (PlaceRight)
                {
                    Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, m_rightWeight);
                    Animator.SetIKRotationWeight(AvatarIKGoal.RightHand, m_rightWeight);
                    Animator.SetIKPosition(AvatarIKGoal.RightHand, currentRightTarget.position);
                    Animator.SetIKRotation(AvatarIKGoal.RightHand, currentRightTarget.rotation);
                }
            }
            else
            {
                if (PlaceLeft)
                {
                    m_leftWeight += WeightSpeed * Time.deltaTime;
                    m_leftWeight = Mathf.Clamp(m_leftWeight, 0, 1);
                    Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, m_leftWeight);
                    Animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, m_leftWeight);
                    Animator.SetIKPosition(AvatarIKGoal.LeftHand, currentLeftTarget.position);
                    Animator.SetIKRotation(AvatarIKGoal.LeftHand, currentLeftTarget.rotation);
                }
                else
                {
                    m_leftWeight -= WeightSpeed * Time.deltaTime;
                    m_leftWeight = Mathf.Clamp(m_leftWeight, 0, 1);
                    Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, m_leftWeight);
                    Animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, m_leftWeight);
                }

                if (PlaceRight)
                {
                    m_rightWeight += WeightSpeed * Time.deltaTime;
                    m_rightWeight = Mathf.Clamp(m_rightWeight, 0, 1);
                    Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, m_rightWeight);
                    Animator.SetIKRotationWeight(AvatarIKGoal.RightHand, m_rightWeight);
                    Animator.SetIKPosition(AvatarIKGoal.RightHand, currentRightTarget.position);
                    Animator.SetIKRotation(AvatarIKGoal.RightHand, currentRightTarget.rotation);
                }
                else
                {
                    m_rightWeight -= WeightSpeed * Time.deltaTime;
                    m_rightWeight = Mathf.Clamp(m_rightWeight, 0, 1);
                    Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, m_rightWeight);
                    Animator.SetIKRotationWeight(AvatarIKGoal.RightHand, m_rightWeight);
                }
            }
        }

        if(BodyLook)
        {
            Animator.SetLookAtPosition(BodyLookAtPosition);
            Animator.SetLookAtWeight(1, LookAtWeight, 0, 0);
        }

        if(lookElbows)
        {
            Animator.SetIKHintPosition(AvatarIKHint.LeftElbow, leftElbowPosition.position);
            Animator.SetIKHintPosition(AvatarIKHint.RightElbow, rightElbowPosition.position);
            Animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 1);
            Animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 1);
        }
    }

   

    public void SetElbows(Transform left, Transform right , bool value)
    {
        rightElbowPosition = right;
        leftElbowPosition = left;
        lookElbows = value;
    }

    public void ResetElbows()
    {
        Animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0);
        Animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0);
        lookElbows = false;
    }

    public void SetWeightSpeed(float value)
    {
        WeightSpeed = value;
    }

    public void ResetWeightSpeed()
    {
        WeightSpeed = m_defaultWeightSpeed;
    }

    public void DisableHandIK()
    {
        PlaceLeft = false;
        PlaceRight = false;
    }

    public void SetBodyLook(Vector3 position, bool value, float weight)
    {
        BodyLook = value;
        BodyLookAtPosition = position;
        LookAtWeight = weight;
    }
    public void ResetBodyLook()
    {
        BodyLook = false;
        LookAtWeight = 0;
    }
}
